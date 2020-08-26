using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("User Publish File Management")]
	public class UserPublishedFileManagementController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly IAmazonS3 _s3Client;

		private Dictionary<string, string> projectFileNamingRules = new Dictionary<string, string> { };

		public UserPublishedFileManagementController(IAmazonS3 s3Client)
		{
			_dbHelper = new DatabaseHelper();
			_s3Client = s3Client;
		}


		[HttpPost]
		[Route("CreateUserPublishedFile")]
		public IActionResult Post(UserPublishedFile request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"folder_content_id", "publish_datetime", "publish_status",
					"published_file_hash", "published_filename", "user_device_id",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create user device content
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var userPublishFileId = request.user_published_file_id ?? Guid.NewGuid().ToString();

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM user_device_contents WHERE user_published_file_id='{userPublishFileId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { user_published_file_id = userPublishFileId, status = "duplicated" });
					}

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM user_devices WHERE user_device_id='{request.user_device_id}')";

					if ((bool)cmd.ExecuteScalar() == false)
					{
						return BadRequest(new { status = "user_device_id doesn't exist" });
					}

					cmd.CommandText = "INSERT INTO user_device_contents "
						+ "(user_device_id, folder_content_id, publish_datetime, publish_status, "
						+ "published_filename, published_file_hash, user_published_file_id) "
						+ "VALUES(@user_device_id, @folder_content_id, @publish_datetime, @publish_status, "
						+ "@published_filename, @published_file_hash, @user_published_file_id)";

					cmd.Parameters.AddWithValue("user_device_id", request.user_device_id);
					cmd.Parameters.AddWithValue("folder_content_id", request.folder_content_id);
					cmd.Parameters.AddWithValue("publish_datetime", DateTimeHelper.ConvertToUTCDateTime(request.publish_datetime));
					cmd.Parameters.AddWithValue("publish_status", request.publish_status);
					cmd.Parameters.AddWithValue("published_filename", request.published_filename);
					cmd.Parameters.AddWithValue("published_file_hash", request.published_file_hash);
					cmd.Parameters.AddWithValue("user_published_file_id", userPublishFileId);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						user_published_file_id = userPublishFileId,
						status = "completed"
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("CreateUserFavorite")]
		public IActionResult Post(UserFavorite request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"favorite_id", "favorite_type", "user_id",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create favorite
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var userFavoriteId = request.user_favorite_id ?? Guid.NewGuid().ToString();
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = $"SELECT user_favorite_id FROM user_favorites WHERE user_id='{request.user_id}' AND favorite_id='{request.favorite_id}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var existingId = _dbHelper.SafeGetString(reader, 0);

							reader.Close();

							cmd.CommandText = $"UPDATE user_favorites SET status='active', edit_datetime=@edit_datetime WHERE user_favorite_id='{existingId}'";
							cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);
							cmd.ExecuteNonQuery();

							return Ok(new { user_favorite_id = existingId, status = "updated" });
						}
					}

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM user_favorites WHERE user_favorite_id='{userFavoriteId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { user_favorite_id = userFavoriteId, status = "duplicated" });
					}

					cmd.CommandText = "INSERT INTO user_favorites "
							+ "(favorite_type, favorite_id, user_id, create_datetime, edit_datetime, user_favorite_id, status, project_id, file_id, favorite_name, favorite_displayname) "
							+ "VALUES(@favorite_type, @favorite_id, @user_id, @create_datetime, @edit_datetime, @user_favorite_id, @status, @project_id, @file_id, @favorite_name, @favorite_displayname) ";

					cmd.Parameters.AddWithValue("@favorite_type", request.favorite_type);
					cmd.Parameters.AddWithValue("@favorite_id", request.favorite_id);
					cmd.Parameters.AddWithValue("@user_id", request.user_id);
					cmd.Parameters.AddWithValue("@create_datetime", timestamp);
					cmd.Parameters.AddWithValue("@edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("@user_favorite_id", userFavoriteId);
					cmd.Parameters.AddWithValue("@status", request.status ?? "active");
					cmd.Parameters.AddWithValue("@project_id", request.project_id ?? "");
					cmd.Parameters.AddWithValue("@file_id", request.file_id ?? "");
					cmd.Parameters.AddWithValue("@favorite_name", request.favorite_name ?? "");
					cmd.Parameters.AddWithValue("@favorite_displayname", request.favorite_displayname ?? "");

					cmd.ExecuteReader();

					return Ok(new
					{
						status = "completed",
						user_favorite_id = userFavoriteId
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("CreateUserDevice")]
		public IActionResult Post(UserDevice request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
					"device_name", "device_type", "user_id", "physical_device_id",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create device
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var userDeviceId = request.user_device_id ?? Guid.NewGuid().ToString();
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM user_devices WHERE user_device_id='{userDeviceId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new
						{
							user_device_id = userDeviceId,
							status = "duplicated"
						});
					}

					if (!string.IsNullOrEmpty(request.physical_device_id))
					{
						cmd.CommandText = $"SELECT user_device_id FROM user_devices WHERE physical_device_id='{request.physical_device_id}'";

						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								var deviceId = _dbHelper.SafeGetString(reader, 0);

								return Ok(new
								{
									user_device_id = deviceId,
									status = "duplicated"
								});
							}
						}
					}

					cmd.CommandText = "INSERT INTO user_devices "
						+ "(device_name, device_description, device_last_update_datetime, device_local_root_path, "
						+ "device_night_end_time, device_night_start_time, device_type, device_update_count, physical_device_id, "
						+ "user_device_id, user_id, status, create_datetime, edit_datetime) "
						+ "VALUES(@device_name, @device_description, @device_last_update_datetime, @device_local_root_path, "
						+ "@device_night_end_time::time, @device_night_start_time::time, @device_type, @device_update_count, @physical_device_id, "
						+ "@user_device_id, @user_id, @status, @create_datetime, @edit_datetime)";

					cmd.Parameters.AddWithValue("device_name", request.device_name);
					cmd.Parameters.AddWithValue("device_description", request.device_description ?? "");
					cmd.Parameters.AddWithValue(
						"device_last_update_datetime",
						request.device_last_update_datetime != null
						? (object)DateTimeHelper.ConvertToUTCDateTime(request.device_last_update_datetime)
						: DBNull.Value);
					cmd.Parameters.AddWithValue("device_local_root_path", request.device_local_root_path ?? "");
					cmd.Parameters.AddWithValue(
						"device_night_end_time",
						request.device_night_end_time != null
						? (object)DateTimeHelper.ConvertToTimeSpan(request.device_night_end_time)
						: DBNull.Value);
					cmd.Parameters.AddWithValue(
						"device_night_start_time",
						request.device_night_start_time != null
						? (object)DateTimeHelper.ConvertToTimeSpan(request.device_night_start_time)
						: DBNull.Value);
					cmd.Parameters.AddWithValue("device_type", request.device_type);
					cmd.Parameters.AddWithValue("device_update_count", request.device_update_count);
					cmd.Parameters.AddWithValue("physical_device_id", request.physical_device_id ?? "");
					cmd.Parameters.AddWithValue("user_device_id", userDeviceId);
					cmd.Parameters.AddWithValue("user_id", request.user_id);
					cmd.Parameters.AddWithValue("status", request.status ?? "active");
					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						status = "completed",
						user_device_id = userDeviceId
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpGet]
		[Route("GetUserDevice")]
		public IActionResult Get(UserDeviceGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.user_device_id))
				{
					return BadRequest(new
					{
						status = "Please provide user_device_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT create_datetime, create_user_id, device_name, device_description, "
						+ "device_last_update_datetime, device_local_root_path, device_night_start_time, device_night_end_time, "
						+ "device_type, device_update_count, edit_datetime, edit_userid, physical_device_id, status, user_device_id, user_id, device_last_seq_num "
						+ "FROM user_devices WHERE user_device_id='" + request.user_device_id + "'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return Ok(new
							{
								create_datetime = _dbHelper.SafeGetDatetimeString(reader, 0),
								create_user_id = _dbHelper.SafeGetString(reader, 1),
								device_name = _dbHelper.SafeGetString(reader, 2),
								device_description = _dbHelper.SafeGetString(reader, 3),
								device_last_update_datetime = _dbHelper.SafeGetDatetimeString(reader, 4),
								device_local_root_path = _dbHelper.SafeGetString(reader, 5),
								device_night_start_time = _dbHelper.SafeGetTimeString(reader, 6),
								device_night_end_time = _dbHelper.SafeGetTimeString(reader, 7),
								device_type = _dbHelper.SafeGetString(reader, 8),
								device_update_count = _dbHelper.SafeGetIntegerRaw(reader, 9),
								edit_datetime = _dbHelper.SafeGetDatetimeString(reader, 10),
								edit_userid = _dbHelper.SafeGetString(reader, 11),
								physical_device_id = _dbHelper.SafeGetString(reader, 12),
								status = _dbHelper.SafeGetString(reader, 13),
								user_device_id = _dbHelper.SafeGetString(reader, 14),
								user_id = _dbHelper.SafeGetString(reader, 15),
								device_last_seq_num = _dbHelper.SafeGetIntegerRaw(reader, 16),
							});
						}
						else
						{
							return BadRequest(new
							{
								status = "User device not found"
							});
						}
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindUserDevices")]
		public IActionResult Get(UserDeviceFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.user_device_id) && string.IsNullOrEmpty(request.user_id))
				{
					return BadRequest(new
					{
						status = "Please provide user_device_id or user_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = " WHERE ";
					if (!string.IsNullOrEmpty(request.user_device_id))
					{
						where += $"user_device_id='{request.user_device_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.user_id))
					{
						where += $"user_id='{request.user_id}' AND ";
					}
					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT create_datetime, create_user_id, device_name, device_description, "
													+ "device_last_update_datetime, device_local_root_path, device_night_start_time, device_night_end_time, "
													+ "device_type, device_update_count, edit_datetime, edit_userid, physical_device_id, status, user_device_id, user_id, device_last_seq_num "
													+ "FROM user_devices" + where;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, object>> { };
						while (reader.Read())
						{
							result.Add(new Dictionary<string, object>
							{
								["create_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 0),
								["create_user_id"] = _dbHelper.SafeGetString(reader, 1),
								["device_name"] = _dbHelper.SafeGetString(reader, 2),
								["device_description"] = _dbHelper.SafeGetString(reader, 3),
								["device_last_update_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 4),
								["device_local_root_path"] = _dbHelper.SafeGetString(reader, 5),
								["device_night_start_time"] = _dbHelper.SafeGetTimeString(reader, 6),
								["device_night_end_time"] = _dbHelper.SafeGetTimeString(reader, 7),
								["device_type"] = _dbHelper.SafeGetString(reader, 8),
								["device_update_count"] = _dbHelper.SafeGetIntegerRaw(reader, 9),
								["edit_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 10),
								["edit_userid"] = _dbHelper.SafeGetString(reader, 11),
								["physical_device_id"] = _dbHelper.SafeGetString(reader, 12),
								["status"] = _dbHelper.SafeGetString(reader, 13),
								["user_device_id"] = _dbHelper.SafeGetString(reader, 14),
								["user_id"] = _dbHelper.SafeGetString(reader, 15),
								["device_last_seq_num"] = _dbHelper.SafeGetIntegerRaw(reader, 16),
							});
						}

						return Ok(result);
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("CreateDocumentLink")]
		public IActionResult Post(DocumentLink request)
		{
			try
			{
				// check missing parameter
				var missingParameter = request.CheckRequiredParameters(new string[]
				{
										"primary_doc_id",
										"linked_doc_id",
				});

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				// create document link
				using (var cmd = _dbHelper.SpawnCommand())
				{
					var documentLinkId = request.document_link_id ?? Guid.NewGuid().ToString();
					var timestamp = DateTime.UtcNow;

					cmd.CommandText = $"SELECT EXISTS (SELECT true FROM document_links WHERE document_link_id='{documentLinkId}')";

					if ((bool)cmd.ExecuteScalar() == true)
					{
						return Ok(new { document_link_id = documentLinkId, status = "duplicated" });
					}

					cmd.CommandText = "INSERT INTO document_links "
						+ "(create_datetime, document_link_id, edit_datetime, link_name, return_link_name, linked_doc_id, primary_doc_id, status) "
						+ "VALUES(@create_datetime, @document_link_id, @edit_datetime, @link_name, @return_link_name, @linked_doc_id, @primary_doc_id, @status)";

					cmd.Parameters.AddWithValue("create_datetime", timestamp);
					cmd.Parameters.AddWithValue("document_link_id", documentLinkId);
					cmd.Parameters.AddWithValue("edit_datetime", timestamp);
					cmd.Parameters.AddWithValue("link_name", request.link_name ?? "");
					cmd.Parameters.AddWithValue("return_link_name", request.return_link_name ?? "");
					cmd.Parameters.AddWithValue("linked_doc_id", request.linked_doc_id);
					cmd.Parameters.AddWithValue("primary_doc_id", request.primary_doc_id);
					cmd.Parameters.AddWithValue("status", request.status ?? "active");

					cmd.ExecuteNonQuery();

					return Ok(new
					{
						document_link_id = documentLinkId,
						status = "completed"
					});
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpGet]
		[Route("FindDocumentLinks")]
		public IActionResult Get(DocumentLinkFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.primary_doc_id))
				{
					return BadRequest(new
					{
						status = "Please provide primary_doc_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT create_datetime, create_user_id, document_link_id, edit_datetime, "
							+ "edit_user_id, link_name, linked_doc_id, status "
							+ $"FROM document_links WHERE status='active' AND primary_doc_id='{request.primary_doc_id}'";

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, string>
							{
								["create_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 0),
								["create_user_id"] = _dbHelper.SafeGetString(reader, 1),
								["document_link_id"] = _dbHelper.SafeGetString(reader, 2),
								["edit_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 3),
								["edit_user_id"] = _dbHelper.SafeGetString(reader, 4),
								["link_name"] = _dbHelper.SafeGetString(reader, 5),
								["linked_doc_id"] = _dbHelper.SafeGetString(reader, 6),
								["status"] = _dbHelper.SafeGetString(reader, 7),
							});
						}

						return Ok(result);
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpGet]
		[Route("FindUserFavorites")]
		public IActionResult Get(UserFavoriteFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.user_id))
				{
					return BadRequest(new
					{
						status = "Please provide user_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = $" WHERE status='active' AND user_id='{request.user_id}' AND ";

					if (!string.IsNullOrEmpty(request.favorite_id))
					{
						where += $" favorite_id='{request.favorite_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.favorite_type))
					{
						where += $" favorite_type='{request.favorite_type}' AND ";
					}
					if (!string.IsNullOrEmpty(request.project_id))
					{
						where += $" project_id='{request.project_id}' AND ";
					}

					where = where.Remove(where.Length - 5);
					
					cmd.CommandText = "SELECT create_datetime, edit_datetime, favorite_id, favorite_type, status, "
						+ "user_favorite_id, project_id, favorite_displayname, favorite_name, file_id "
						+ "FROM user_favorites "
						+ where;

					using (var reader = cmd.ExecuteReader())
					{
						var result = new List<Dictionary<string, string>> { };

						while (reader.Read())
						{
							result.Add(new Dictionary<string, string>
							{
								["create_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 0),
								["edit_datetime"] = _dbHelper.SafeGetDatetimeString(reader, 1),
								["favorite_id"] = _dbHelper.SafeGetString(reader, 2),
								["favorite_type"] = _dbHelper.SafeGetString(reader, 3),
								["status"] = _dbHelper.SafeGetString(reader, 4),
								["user_favorite_id"] = _dbHelper.SafeGetString(reader, 5),
								["project_id"] = _dbHelper.SafeGetString(reader, 6),
								["favorite_displayname"] = _dbHelper.SafeGetString(reader, 7),
								["favorite_name"] = _dbHelper.SafeGetString(reader, 8),
								["file_id"] = _dbHelper.SafeGetString(reader, 9)
							});
						}

						return Ok(result);
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpPost]
		[Route("RemoveUserFavorite")]
		public IActionResult Post(UserFavoriteRemoveRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.user_favorite_id))
				{
					return BadRequest(new
					{
						status = "Please provide user_favorite_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"UPDATE user_favorites SET status='deleted' WHERE user_favorite_id='{request.user_favorite_id}'";

					var updatedCount = cmd.ExecuteNonQuery();

					if (updatedCount == 0)
					{
						return BadRequest(new
						{
							status = "user_favorite_id not found"
						});
					}
					else
					{
						return Ok(new
						{
							status = "completed"
						});
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("UpdateDocumentLink")]
		public IActionResult Post(DocumentLinkUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.search_document_link_id))
				{
					return BadRequest(new { status = "Please provide search_document_link_id" });
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE document_links SET "
							+ "link_name = COALESCE(@link_name, link_name), "
							+ "status = COALESCE(@status, status), "
							+ "edit_datetime = @edit_datetime "
							+ $"WHERE document_link_id='{request.search_document_link_id}'";

					cmd.Parameters.AddWithValue("link_name", (object)request.link_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					var updatedCount = cmd.ExecuteNonQuery();

					if (updatedCount == 0)
					{
						return BadRequest(new
						{
							status = "document_link_id not found"
						});
					}
					else
					{
						return Ok(new
						{
							status = "completed"
						});
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		[HttpPost]
		[Route("UpdateUserDevice")]
		public IActionResult Post(UserDeviceUpdateRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.search_user_device_id))
				{
					return BadRequest(new
					{
						status = "Please provide search_user_device_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = "UPDATE user_devices SET "
							+ "device_name = COALESCE(@device_name, device_name), "
							+ "device_description = COALESCE(@device_description, device_description), "
							+ "device_type = COALESCE(@device_type, device_type), "
							+ "device_last_update_datetime = COALESCE(@device_last_update_datetime, device_last_update_datetime), "
							+ "device_local_root_path = COALESCE(@device_local_root_path, device_local_root_path), "
							+ "device_night_start_time = COALESCE(@device_night_start_time::time, device_night_start_time), "
							+ "device_night_end_time = COALESCE(@device_night_end_time::time, device_night_end_time), "
							+ "device_update_count = COALESCE(@device_update_count, device_update_count), "
							+ "device_last_seq_num = COALESCE(@device_last_seq_num, device_last_seq_num), "
							+ "physical_device_id = COALESCE(@physical_device_id, physical_device_id), "
							+ "user_id = COALESCE(@user_id, user_id), "
							+ "status = COALESCE(@status, status), "
							+ "edit_datetime = COALESCE(@edit_datetime, edit_datetime) "
							+ $"WHERE user_device_id='{request.search_user_device_id}'";

					cmd.Parameters.AddWithValue("device_name", (object)request.device_name ?? DBNull.Value);
					cmd.Parameters.AddWithValue("device_description", (object)request.device_description ?? DBNull.Value);
					cmd.Parameters.AddWithValue("device_type", (object)request.device_type ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_last_update_datetime",
									request.device_last_update_datetime != null
									? (object)DateTimeHelper.ConvertToUTCDateTime(request.device_last_update_datetime)
									: DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_local_root_path",
									(object)request.device_local_root_path ?? DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_night_start_time",
									request.device_night_start_time != null
									? (object)DateTimeHelper.ConvertToTimeSpan(request.device_night_start_time)
									: DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_night_end_time",
									request.device_night_end_time != null
									? (object)DateTimeHelper.ConvertToTimeSpan(request.device_night_end_time)
									: DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_update_count",
									request.device_update_count >= 0
									? (object)request.device_update_count
									: DBNull.Value);
					cmd.Parameters.AddWithValue(
									"device_last_seq_num",
									request.device_last_seq_num >= 0 ? (object)request.device_last_seq_num : DBNull.Value);
					cmd.Parameters.AddWithValue("physical_device_id", (object)request.physical_device_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("user_id", (object)request.user_id ?? DBNull.Value);
					cmd.Parameters.AddWithValue("status", (object)request.status ?? DBNull.Value);
					cmd.Parameters.AddWithValue("edit_datetime", DateTime.UtcNow);

					var updatedCount = cmd.ExecuteNonQuery();

					if (updatedCount == 0)
					{
						return BadRequest(new
						{
							status = "user_device_id not found"
						});
					}
					else
					{
						return Ok(new
						{
							status = "completed"
						});
					}
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		[HttpGet]
		[Route("FindUnpublishedUserFiles")]
		public IActionResult Get(UnpublishedFindRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.user_device_id))
				{
					return BadRequest(new
					{
						status = "Please provide user_device_id"
					});
				}

				using (var cmd = _dbHelper.SpawnCommand())
				{
					var where = $" WHERE folder_transaction_log.folder_transaction_sequence_num > {request.last_sync_sequence_num} AND ";
					var deviceLocalRootPath = "";

					cmd.CommandText = $"SELECT device_local_root_path FROM user_devices WHERE user_device_id='{request.user_device_id}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							deviceLocalRootPath = _dbHelper.SafeGetString(reader, 0);
						}
						else
						{
							return BadRequest(new
							{
								status = "user_device not found"
							});
						}
					}

					if (!string.IsNullOrEmpty(request.folder_id))
					{
						where += $"folder_transaction_log.folder_id='{request.folder_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.project_id))
					{
						where += $"folder_transaction_log.project_id='{request.project_id}' AND ";
					}

					if (!string.IsNullOrEmpty(request.customer_id))
					{
						where += $"folder_transaction_log.customer_id='{request.customer_id}' AND ";
					}
					if (!string.IsNullOrEmpty(request.office_id))
					{
						where += $"folder_transaction_log.office_id='{request.office_id}' AND ";
					}

					where = where.Remove(where.Length - 5);

					cmd.CommandText = "SELECT folder_transaction_log.doc_id, project_documents.doc_name, project_documents.doc_number, "
																					+ "project_documents.doc_revision, project_documents.doc_type, folder_transaction_log.file_id, files.file_original_filename, folder_transaction_log.project_id, "
																					+ "files.file_type, projects.project_name, project_folders.folder_type, folder_transaction_log.folder_path, "
																					+ "files.file_size, folder_transaction_log.operation_type, folder_transaction_log.folder_transaction_sequence_num, folder_transaction_log.original_folder_name, folder_transaction_log.new_folder_name, folder_transaction_log.folder_id, project_documents.status "
																					+ "FROM folder_transaction_log "
																					+ "LEFT JOIN project_documents ON project_documents.doc_id=folder_transaction_log.doc_id "
																					+ "LEFT JOIN files ON files.file_id=folder_transaction_log.file_id "
																					+ "LEFT JOIN projects ON projects.project_id=folder_transaction_log.project_id "
																					+ "LEFT JOIN project_folders ON project_folders.folder_id=folder_transaction_log.folder_id "
																					+ where + " ORDER BY folder_transaction_log.folder_transaction_sequence_num ASC";

					var result = new List<Dictionary<string, object>> { };

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var folderPath = _dbHelper.SafeGetString(reader, 11);
							var projectName = _dbHelper.SafeGetString(reader, 9);
							var folderType = _dbHelper.SafeGetString(reader, 10);
							var fileType = _dbHelper.SafeGetString(reader, 8);
							var rootFolderName = DocumentManagementController.__getRootFolderName(folderType);

							folderPath = string.Join("/", folderPath.Split("/").Select(path => path.Trim()));

							result.Add(new Dictionary<string, object>
							{
								["doc_id"] = _dbHelper.SafeGetString(reader, 0),
								["doc_name"] = _dbHelper.SafeGetString(reader, 1),
								["doc_number"] = _dbHelper.SafeGetString(reader, 2),
								["doc_revision"] = _dbHelper.SafeGetString(reader, 3),
								["file_hash"] = _dbHelper.SafeGetString(reader, 5),
								["file_id"] = _dbHelper.SafeGetString(reader, 5),
								["file_path"] = $"{deviceLocalRootPath}/{projectName}/{rootFolderName}/{folderPath}",
								["file_size"] = _dbHelper.SafeGetString(reader, 12),
								["project_id"] = _dbHelper.SafeGetString(reader, 7),
								["original_filename"] = _dbHelper.SafeGetString(reader, 6),
								["doc_type"] = _dbHelper.SafeGetString(reader, 4),
								["folder_type"] = folderType,
								["file_type"] = fileType,
								["operation_type"] = _dbHelper.SafeGetString(reader, 13),
								["project_name"] = projectName,
								["folder_transaction_sequence_num"] = _dbHelper.SafeGetIntegerRaw(reader, 14),
								["original_folder_name"] = _dbHelper.SafeGetString(reader, 15),
								["new_folder_name"] = _dbHelper.SafeGetString(reader, 16),
								["folder_id"] = _dbHelper.SafeGetString(reader, 17),
								["status"] = _dbHelper.SafeGetString(reader, 18),
							});
						}
					}

					result.ForEach(dict =>
					{
						if ((dict["doc_type"] as string).Contains("single_sheet_plan") && !(dict["folder_type"] as string).Contains("source_"))
						{
							dict["file_name"] = __getFileName(dict["doc_name"] as string, dict["doc_number"] as string, dict["doc_revision"] as string, dict["original_filename"] as string, dict["project_id"] as string, dict["file_type"] as string);
						}
						else
						{
							dict["file_name"] = dict["original_filename"];
						}

						dict.Remove("original_filename");
						dict.Remove("doc_type");
						dict.Remove("file_type");
					});
					return Ok(result);
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		// NOTE: DEPRECATED
		/*
[HttpGet]
[Route("GetUserPublishedCurrentPlans")]
public IActionResult Get(UserPublishedCurrentPlansGetRequest request)
{
try
{
		if (string.IsNullOrEmpty(request.user_device_id))
		{
				return BadRequest(new { status = "Please provide user_device_id" });
		}

		using (var cmd = _dbHelper.SpawnCommand())
		{
				var where = " WHERE project_folder_contents.status='active' AND project_folders.folder_type='plans_current' AND ";
				var deviceLocalRootPath = "";

				cmd.CommandText = "SELECT users.customer_id, user_devices.user_id, user_devices.device_local_root_path FROM user_devices "
						+ "LEFT OUTER JOIN users ON users.user_id=user_devices.user_id "
						+ "WHERE user_device_id='" + request.user_device_id + "'";

				using (var reader = cmd.ExecuteReader())
				{
						if (reader.Read())
						{
								var customerId = _dbHelper.SafeGetString(reader, 0);
								var userId = _dbHelper.SafeGetString(reader, 1);
								deviceLocalRootPath = _dbHelper.SafeGetString(reader, 2);

								if (!string.IsNullOrEmpty(customerId))
								{
										where += "users.customer_id='" + customerId + "' AND ";
								}
								else
								{
										where += "projects.project_admin_user='" + userId + "' AND ";
								}
						}
						else
						{
								return BadRequest(new { status = "user_device not found" });
						}
				}

				if (!string.IsNullOrEmpty(request.folder_id))
				{
						where += "project_folder_contents.folder_id='" + request.folder_id + "' AND ";
				}
				if (!string.IsNullOrEmpty(request.project_id))
				{
						where += "projects.project_id='" + request.project_id + "' AND ";
				}

				where = where.Remove(where.Length - 5);

				cmd.CommandText = "SELECT project_folder_contents.folder_content_id, project_folder_contents.doc_id, "
						+ "project_documents.doc_name, project_folder_contents.file_id AS file_hash, "
						+ "project_folder_contents.file_id AS file_id, files.file_original_filename, "
						+ "project_folder_contents.folder_path, files.file_size, projects.project_admin_user_id, "
						+ "user_devices.user_device_id, project_folder_contents.status, projects.project_name, projects.project_id, "
						+ "project_documents.doc_number, project_documents.doc_revision, project_documents.doc_type, project_folders.folder_type, files.file_type "
						+ "FROM project_folder_contents "
						+ "LEFT OUTER JOIN user_device_contents ON user_device_contents.folder_content_id=project_folder_contents.folder_content_id "
						+ "LEFT OUTER JOIN user_devices ON user_devices.user_device_id=user_device_contents.user_device_id "
						+ "LEFT OUTER JOIN project_documents ON project_documents.doc_id=project_folder_contents.doc_id "
						+ "LEFT OUTER JOIN files ON files.file_id=project_folder_contents.file_id "
						+ "LEFT OUTER JOIN project_folders ON project_folders.folder_id=project_folder_contents.folder_id "
						+ "LEFT OUTER JOIN projects ON projects.project_id=project_folders.project_id "
						+ "LEFT OUTER JOIN users ON users.user_id=projects.project_admin_user_id "
						+ where;

				var result = new List<Dictionary<string, string>> { };

				using (var reader = cmd.ExecuteReader())
				{
						while (reader.Read())
						{
								var folderPath = _dbHelper.SafeGetString(reader, 6);
								var projectName = _dbHelper.SafeGetString(reader, 11);
								var folderType = _dbHelper.SafeGetString(reader, 16);
								var fileType = _dbHelper.SafeGetString(reader, 17);
								var rootFolderName = DocumentManagementController.__getRootFolderName(folderType);

								folderPath = string.Join("/", folderPath.Split("/").Select(path => path.Trim()));

								result.Add(new Dictionary<string, string>
								{
										["doc_id"] = _dbHelper.SafeGetString(reader, 1),
										["doc_name"] = _dbHelper.SafeGetString(reader, 2),
										["file_hash"] = _dbHelper.SafeGetString(reader, 3),
										["file_id"] = _dbHelper.SafeGetString(reader, 3),
										["file_path"] = $"{deviceLocalRootPath}/{projectName}/{rootFolderName}/{folderPath}",
										["file_size"] = _dbHelper.SafeGetString(reader, 7),
										["status"] = _dbHelper.SafeGetString(reader, 10),
										["folder_content_id"] = _dbHelper.SafeGetString(reader, 0),
										["project_id"] = _dbHelper.SafeGetString(reader, 12),
										["doc_number"] = _dbHelper.SafeGetString(reader, 13),
										["doc_revision"] = _dbHelper.SafeGetString(reader, 14),
										["original_filename"] = _dbHelper.SafeGetString(reader, 5),
										["doc_type"] = _dbHelper.SafeGetString(reader, 15),
										["folder_type"] = folderType,
										["file_type"] = fileType,
								});
						}
				}

				result.ForEach(dict =>
				{
						if (dict["doc_type"].Contains("single_sheet_plan") && !dict["folder_type"].Contains("source_"))
						{
								dict["file_name"] = dict["file_name"] = __getFileName(dict["doc_name"], dict["doc_number"], dict["doc_revision"], dict["original_filename"], dict["project_id"], dict["file_type"]);
						}
						else
						{
								dict["file_name"] = dict["original_filename"];
						}

						dict.Remove("doc_number");
						dict.Remove("doc_revision");
						dict.Remove("original_filename");
						dict.Remove("doc_type");
						dict.Remove("folder_type");
						dict.Remove("file_type");
				});

				return Ok(result);
		}
}
catch (Exception exception)
{
		return BadRequest(new { status = exception.Message });
}
finally
{
		_dbHelper.CloseConnection();
}
}
*/

		[HttpGet]
		[Route("GetFile")]
		public async Task<IActionResult> GetAsync(FileGetRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.file_id))
				{
					return BadRequest(new
					{
						status = "Please provide file_id"
					});
				}

				var bucketName = __getBucketName();

				if (string.IsNullOrEmpty(bucketName))
				{
					return BadRequest(new
					{
						status = "Failed to read bucket name from system settings"
					});
				}

				var fileKey = __getFileKey(request.file_id);

				if (string.IsNullOrEmpty(fileKey))
				{
					return BadRequest(new
					{
						status = "file_key not found with given file_id"
					});
				}

				var fileStream = await _s3Client.GetObjectStreamAsync(bucketName, fileKey, null);

				if (fileStream == null)
				{
					return BadRequest(new
					{
						status = "Failed to download file from the bucket"
					});
				}

				var fileExtension = StringHelper.GetFileExtension(fileKey);

				if (string.IsNullOrEmpty(fileExtension))
				{
					return File(fileStream, "application/octet-stream", $"{request.file_id}");
				}
				else
				{
					return File(fileStream, "application/octet-stream", $"{request.file_id}.{fileExtension}");
				}
			}
			catch (Exception exception)
			{
				return BadRequest(new
				{
					status = exception.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}


		private string __getFileKey(string file_id)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT file_key FROM files where file_id='{file_id}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return _dbHelper.SafeGetString(reader, 0);
					}
					else
					{
						return null;
					}
				}
			}
		}

		private string __getBucketName()
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM system_settings WHERE setting_name='BR_PERM_VAULT'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return _dbHelper.SafeGetString(reader, 0);
					}
					else
					{
						return null;
					}
				}
			}
		}

		private string __getFileName(
						string doc_name,
						string doc_number,
						string doc_revision,
						string originalFileName,
						string projectId,
						string file_type)
		{
			var fileNamingRule = "";
			var fileExtension = StringHelper.GetFileExtension(originalFileName) ?? "";

			if (projectFileNamingRules.ContainsKey(projectId))
			{
				fileNamingRule = projectFileNamingRules[projectId];
			}
			else
			{
				using (var cmd = _dbHelper.SpawnCommand())
				{
					cmd.CommandText = $"SELECT setting_value FROM project_settings WHERE setting_name='PROJECT_PLAN_FILE_NAMING' and project_id='{projectId}'";

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							fileNamingRule = _dbHelper.SafeGetString(reader, 0);
						}
					}

					if (string.IsNullOrEmpty(fileNamingRule))
					{
						projectFileNamingRules[projectId] = "<doc_num>__<doc_revision>";
					}
					else
					{
						projectFileNamingRules[projectId] = fileNamingRule;
					}
				}
			}

			var fileName = fileNamingRule
											.Replace("<doc_num>", doc_number)
											.Replace("<doc_name>", doc_name)
											.Replace("<doc_revision>", doc_revision);

			if (file_type == "comparison_file")
			{
				fileName += "_comparison";
			}

			if (!string.IsNullOrEmpty(fileExtension))
			{
				fileName += $".{fileExtension}";
			}

			return fileName;
		}
	}
}
