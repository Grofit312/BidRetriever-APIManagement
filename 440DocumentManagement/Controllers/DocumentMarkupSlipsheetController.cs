using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DocMarkupAnnotations;
using _440DocumentManagement.Models.DocumentMarkupSlipsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using NSwag.Annotations;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	[OpenApiTag("Document Markup Slipsheet Management")]
	public class DocumentMarkupSlipsheetController : Controller
	{
		private readonly DatabaseHelper _dbHelper;
		private readonly HttpClient client = new HttpClient();

		public DocumentMarkupSlipsheetController()
		{
			_dbHelper = new DatabaseHelper();
		}

		[HttpPost]
		[Route("CreateDocumentMarkupSlipsheet")]
		public IActionResult CreateDocumentMarkupSlipsheet(DocumentMarkupSlipsheet request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(new
					{
						status = "Request is null."
					});
				}

				// Verify markup_id and doc_id
				var missingParameter = request.CheckRequiredParameters(new string[] { "markup_id", "doc_id" });

				if (missingParameter != null)
				{
					return BadRequest(new
					{
						status = $"{missingParameter} is required"
					});
				}

				//check if document markups exists for the reqested markup_id.
				if (_IsExists(request.markup_id, "markup_id"))
				{
					//check if markup slipsheet exist
					if (_IsExists(request.markup_id, "parent_markup_id"))
					{
						return BadRequest(new
						{
							status = "A slipsheet markup is already existed."
						});
					}
					
					// Slipsheet not exist
					// Get Original Markup
					var responseResult = new DocumentMarkupManagementController().GetDocumentMarkupById(request.markup_id);
					string responseJson = JsonConvert.SerializeObject(responseResult);
					JObject obj = JObject.Parse(responseJson);
					if (Convert.ToString(obj["Value"]["statuscode"]).Equals("200"))
					{
						var responseData = obj["Value"]["data"].FirstOrDefault();
						DocumentMarkup _obj = new DocumentMarkup
						{
							author_companyname = GetStringValue(responseData["author_companyname"]),
							author_displayname = GetStringValue(responseData["author_display_name"]),
							author_userid = GetStringValue(responseData["author_userid"]),
							create_datetime = (DateTime)responseData["create_datetime"],
							create_userid = GetStringValue(responseData["create_userid"]),
							edit_datetime = (DateTime)responseData["edit_datetime"],
							edit_userid = GetStringValue(responseData["edit_userid"]),
							markup_description = GetStringValue(responseData["markup_description"]),
							markup_name = GetStringValue(responseData["markup_name"]),
							status = GetStringValue(responseData["markeup_status"]),
							doc_id = request.doc_id,
							parent_markup_id = request.markup_id,
							file_id = GetStringValue(responseData["file_id"])
						};

						//Create Slipsheet Markup
						var createSlipResponse = new DocumentMarkupManagementController().CreateDocumentMarkup(_obj, true);
						responseJson = JsonConvert.SerializeObject(createSlipResponse);
						obj = JObject.Parse(responseJson);
						responseData = obj["Value"];
						if (Convert.ToString(responseData["statuscode"]).Equals("200"))//Slipsheet created successfully.
						{
							string slipsheet_markup_id = Convert.ToString(responseData["markup_id"]); //Slipsheet markup_id
																																												//Get Document Markup Annotation
							DataTable dt = GetAnnotationsDataTable(request.markup_id, request.active_annotations_only);
							foreach (DataRow dr in dt.Rows)//copy all document markup annotation to slipsheet markup
							{
								string annotation_id = Guid.NewGuid().ToString();
								string query = "INSERT INTO public.document_markup_annotations (annotation_id, annotation_type, create_datetime, create_userid, edit_datetime, edit_userid, markup_id, annotation_current_data, annotation_status, parent_annotation_id) VALUES(@annotation_id, @annotation_type, @create_datetime, @create_userid, @edit_datetime, @edit_userid, @markup_id, @annotation_current_data, @annotation_status, @parent_annotation_id)";
								using (var cmd = _dbHelper.SpawnCommand())
								{
									cmd.Parameters.Clear();
									cmd.Parameters.AddWithValue("@annotation_id", annotation_id);
									cmd.Parameters.AddWithValue("@annotation_type", dr["annotation_type"]);
									cmd.Parameters.AddWithValue("@create_datetime", dr["create_datetime"]);
									cmd.Parameters.AddWithValue("@create_userid", dr["create_userid"]);
									cmd.Parameters.AddWithValue("@edit_datetime", dr["edit_datetime"]);
									cmd.Parameters.AddWithValue("@edit_userid", dr["edit_userid"]);
									cmd.Parameters.AddWithValue("@markup_id", slipsheet_markup_id);
									cmd.Parameters.AddWithValue("@annotation_current_data", dr["annotation_current_data"]);
									cmd.Parameters.AddWithValue("@annotation_status", dr["annotation_status"]);
									cmd.Parameters.AddWithValue("@parent_annotation_id", dr["annotation_id"]);

									cmd.CommandText = query;
									cmd.ExecuteNonQuery();
								}
							}
						}
					}
					return Ok(new
					{
						request.markup_id,
						status = "A slipsheet markup is successfully created."
					});
				}
				else
				{
					return BadRequest(new
					{
						request.markup_id,
						status = "No document markup is existed."
					});
				}
			}
			catch (Exception ex)
			{
				return BadRequest(new
				{
					status = ex.Message
				});
			}
			finally
			{
				_dbHelper.CloseConnection();
			}
		}

		private bool _IsExists(string Id, string columnName)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT EXISTS (SELECT true FROM public.document_markups WHERE {columnName} ='{ Id }')";
				return (bool)cmd.ExecuteScalar();
			}
		}

		private string GetStringValue(JToken token)
		{
			return Convert.ToString(token);
		}

		private DataTable GetAnnotationsDataTable(string markup_id, bool _active_annotations_only)
		{
			using (var cmd = _dbHelper.SpawnCommand())
			{
				string _activecondition = string.Empty;
				//if user specified to copy active annotations only.
				if (_active_annotations_only)
					_activecondition = "AND annotation_status = 'active'";
				cmd.CommandText = "SELECT * FROM public.document_markup_annotations where markup_id = @markup_id " + _activecondition;
				cmd.Parameters.AddWithValue("@markup_id", markup_id);
				NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
				DataTable _dt = new DataTable();
				da.Fill(_dt);
				return _dt;
			}
		}
	}
}
