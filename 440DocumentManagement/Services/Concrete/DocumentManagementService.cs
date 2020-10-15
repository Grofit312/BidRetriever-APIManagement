using _440DocumentManagement.Helpers;
using _440DocumentManagement.Services.Interface;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _440DocumentManagement.Services.Concrete
{
	public class DocumentManagementService : IDocumentManagementService
	{
		private readonly IAmazonS3 _s3Client;

		public DocumentManagementService(
			IAmazonS3 s3Client)
		{
			_s3Client = s3Client;
		}

		~DocumentManagementService() {
		}

		public void CreateFolderTransactionLog(
			DatabaseHelper dbHelper,
			Dictionary<string, object> data, string operationType)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "INSERT INTO folder_transaction_log "
						+ "(project_id, folder_id, operation_type, doc_id, transaction_datetime, folder_path, file_id, original_folder_name, new_folder_name) "
						+ "VALUES(@project_id, @folder_id, @operation_type, @doc_id, @transaction_datetime, @folder_path, @file_id, @original_folder_name, @new_folder_name)";

				cmd.Parameters.AddWithValue("project_id", data["project_id"]);
				cmd.Parameters.AddWithValue("folder_id", data["folder_id"]);
				cmd.Parameters.AddWithValue("operation_type", operationType);
				cmd.Parameters.AddWithValue("doc_id", data["doc_id"]);
				cmd.Parameters.AddWithValue("file_id", data["file_id"]);
				cmd.Parameters.AddWithValue("transaction_datetime", DateTime.UtcNow);
				cmd.Parameters.AddWithValue("folder_path", data["folder_path"]);
				cmd.Parameters.AddWithValue("original_folder_name", data["original_folder_name"]);
				cmd.Parameters.AddWithValue("new_folder_name", data["new_folder_name"]);

				cmd.ExecuteNonQuery();
			}
		}

		public string GenerateDocRevision(
			DatabaseHelper dbHelper,
			string customerId,
			Dictionary<string, object> currentDoc,
			List<Dictionary<string, object>> documents)
		{
			var isCounterBasedRevision = false;
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM customer_settings "
					+ "WHERE setting_id = 'revisioning_type' AND customer_id = @customer_id";
				cmd.Parameters.AddWithValue("@customer_id", customerId);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						isCounterBasedRevision = dbHelper.SafeGetString(reader, 0) == "Submission Counter";
					}
				}
			}

			string updatedDocRevision = "";
			if (isCounterBasedRevision)
			{
				if (documents.Count == 0)
				{
					updatedDocRevision = "00";
				}
				else
				{
					updatedDocRevision = (Int32.Parse((string)documents.Last()["doc_revision"]) + 1).ToString().PadLeft(2, '0');
				}
			}
			else
			{
				var numberOfPreviousDocsInSameDay = documents.Where(prevDoc =>
					((string)prevDoc["submission_datetime"]).Substring(0, 10) == ((string)currentDoc["submission_datetime"]).Substring(0, 10)).ToList().Count;
				if (numberOfPreviousDocsInSameDay == 0)
				{
					updatedDocRevision = ((string)currentDoc["submission_datetime"]).Substring(0, 10);
				}
				else
				{
					updatedDocRevision = DateTimeHelper.ConvertToUTCDateTime((string)currentDoc["submission_datetime"]).ToString("yyyy-MM-dd_HH-mm");
					updatedDocRevision += "-" + numberOfPreviousDocsInSameDay.ToString().PadLeft(2, '0');
				}
			}

			return updatedDocRevision;
		}

		public string GeneratePlanFileName(
			DatabaseHelper dbHelper, string projectId, string docName, string docNumber, string docRevision)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM project_settings "
					+ "WHERE project_id = @project_id AND setting_name = 'PROJECT_PLAN_FILE_NAMING'";
				cmd.Parameters.AddWithValue("@project_id", projectId);

				using (var reader = cmd.ExecuteReader())
				{
					string planFileNamingSetting = null;
					if (reader.Read())
					{
						planFileNamingSetting = dbHelper.SafeGetString(reader, 0);
					}
					var planFileNamingRule = string.IsNullOrEmpty(planFileNamingSetting) ?
						"<doc_num>__<doc_revision>" : planFileNamingSetting;
					return planFileNamingRule
						.Replace("<doc_num>", docNumber)
						.Replace("<doc_revision>", docRevision)
						.Replace("<doc_name>", docName);
				}
			}
		}

		public Dictionary<string, string> GetDocumentComparison(DatabaseHelper dbHelper, string docId, string docRevision)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT file_key, bucket_name FROM project_documents "
						+ "LEFT JOIN document_files ON project_documents.doc_id=document_files.doc_id "
						+ "LEFT JOIN files ON files.file_id=document_files.file_id "
						+ $"WHERE project_documents.doc_id='{docId}' AND file_type='comparison_file' "
						+ "ORDER BY files.create_datetime DESC LIMIT 1";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new Dictionary<string, string>
						{
							["doc_revision"] = docRevision,
							["file_key"] = dbHelper.SafeGetString(reader, 0),
							["bucket_name"] = dbHelper.SafeGetString(reader, 1),
						};
					}
					else
					{
						return new Dictionary<string, string>
						{
							["doc_revision"] = $"{docRevision}_unable to generate",
							["file_key"] = "",
							["bucket_name"] = "",
						};
					}
				}
			}
		}

		public List<Dictionary<string, string>> GetDocumentComparisons(DatabaseHelper dbHelper, List<Dictionary<string, string>> docRevisions)
		{
			if (docRevisions.Count <= 1)
			{
				return new List<Dictionary<string, string>> { };
			}

			var resultList = new List<Dictionary<string, string>> { };

			docRevisions.ForEach(docRevision =>
			{
				resultList.Add(GetDocumentComparison(dbHelper, docRevision["doc_id"], docRevision["doc_revision"]));
			});

			resultList.RemoveAt(0);

			return resultList;
		}

		public Dictionary<string, string> GetDocumentRevision(DatabaseHelper dbHelper, string docId)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT project_documents.doc_id, doc_revision, files.file_key, files.bucket_name FROM project_documents "
						+ "LEFT JOIN document_files ON project_documents.doc_id=document_files.doc_id "
						+ "LEFT JOIN files ON document_files.file_id=files.file_id "
						+ $"WHERE project_documents.doc_id='{docId}' AND files.file_type='source_system_original'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new Dictionary<string, string>
						{
							["doc_id"] = dbHelper.SafeGetString(reader, 0),
							["doc_revision"] = dbHelper.SafeGetString(reader, 1),
							["file_key"] = dbHelper.SafeGetString(reader, 2),
							["bucket_name"] = dbHelper.SafeGetString(reader, 3),
						};
					}
					else
					{
						return new Dictionary<string, string>
						{
							["doc_id"] = "not_found",
							["doc_revision"] = "not_found",
							["file_key"] = "not_found",
							["bucket_name"] = "not_found",
						};
					}
				}
			}
		}

		public List<Dictionary<string, string>> GetDocumentRevisions(DatabaseHelper dbHelper, string docId)
		{
			var resultList = new List<Dictionary<string, string>> { };
			var currentDocId = docId;

			while (true)
			{
				currentDocId = GetPreviousRevisionDocId(dbHelper, currentDocId);

				if (string.IsNullOrEmpty(currentDocId))
				{
					break;
				}
				else
				{
					resultList.Insert(0, GetDocumentRevision(dbHelper, currentDocId));
				}
			}

			currentDocId = docId;
			resultList.Add(GetDocumentRevision(dbHelper, currentDocId));

			while (true)
			{
				currentDocId = GetNextRevisionDocId(dbHelper, currentDocId);

				if (string.IsNullOrEmpty(currentDocId))
				{
					break;
				}
				else
				{
					resultList.Add(GetDocumentRevision(dbHelper, currentDocId));
				}
			}

			return resultList;
		}

		public string GetNextRevisionDocId(DatabaseHelper dbHelper, string currentDocId)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT doc_next_rev FROM project_documents WHERE doc_id='{currentDocId}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var docId = dbHelper.SafeGetString(reader, 0);

						return docId;
					}
					else
					{
						return null;
					}
				}
			}
		}

		public string GetPreviousRevisionDocId(DatabaseHelper dbHelper, string currentDocId)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = $"SELECT doc_id FROM project_documents WHERE doc_next_rev='{currentDocId}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var docId = dbHelper.SafeGetString(reader, 0);

						return docId;
					}
					else
					{
						return null;
					}
				}
			}
		}

		public void RemoveComparison(DatabaseHelper dbHelper, string docId)
		{
			// Get Comparison Details
			Dictionary<string, string> comparisonDetails = null;
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT "
					+ "document_files.file_id, "
					+ "document_files.doc_file_id "
					+ "FROM project_documents "
					+ "LEFT JOIN document_files ON document_files.doc_id = project_documents.doc_id "
					+ "LEFT JOIN files ON files.file_id = document_files.file_id "
					+ "WHERE project_documents.doc_id = @doc_id AND files.file_type = 'comparison_file'";
				cmd.Parameters.AddWithValue("@doc_id", docId);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						comparisonDetails = new Dictionary<string, string>()
						{
							{ "file_id", dbHelper.SafeGetString(reader, 0) },
							{ "doc_file_id", dbHelper.SafeGetString(reader, 1) }
						};
					}
				}
			}
			if (comparisonDetails == null)
			{
				// Comparison is not existed.
				return;
			}

			// Delete Comparison from files table
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "DELETE FROM files where file_id = @file_id";
				cmd.Parameters.AddWithValue("@file_id", comparisonDetails["file_id"]);
				cmd.ExecuteNonQuery();
			}

			// Delete comparison from project_files table
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "DELETE from project_files WHERE doc_file_id = @doc_file_id";
				cmd.Parameters.AddWithValue("@doc_file_id", comparisonDetails["doc_file_id"]);
				cmd.ExecuteNonQuery();
			}

			// Delete folder content for comparison
			using (var cmd = dbHelper.SpawnCommand())
			{
				var folderId = "";
				cmd.CommandText = "SELECT folder_id "
					+ "FROM project_folder_contents WHERE doc_id = @doc_id AND file_id = @file_id";
				cmd.Parameters.AddWithValue("@doc_id", docId);
				cmd.Parameters.AddWithValue("@file_id", comparisonDetails["file_id"]);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						folderId = dbHelper.SafeGetString(reader, 0);
					}
				}

				cmd.Parameters.Clear();
				cmd.CommandText = $"DELETE FROM project_folder_contents WHERE doc_id = @doc_id AND file_id = @file_id";
				cmd.Parameters.AddWithValue("@doc_id", docId);
				cmd.Parameters.AddWithValue("@file_id", comparisonDetails["file_id"]);
				cmd.ExecuteNonQuery();

				cmd.Parameters.Clear();
				cmd.CommandText = "UPDATE project_folders SET folder_content_quantity = folder_content_quantity - 1 "
												+ $"WHERE folder_id='{folderId}';";
				cmd.ExecuteNonQuery();
			}

			// Delete published record for comparison
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "DELETE FROM project_documents_published WHERE doc_id = @doc_id AND file_id = @file_id";
				cmd.Parameters.AddWithValue("@doc_id", docId);
				cmd.Parameters.AddWithValue("@file_id", comparisonDetails["file_id"]);
				cmd.ExecuteNonQuery();
			}
		}

		public Dictionary<string, string> RetrieveDocument(DatabaseHelper dbHelper, string docId)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT "
					+ "project_documents.doc_id, project_documents.project_id, project_documents.doc_number, "
					+ "project_documents.doc_name, project_documents.doc_version, project_documents.doc_revision, "
					+ "project_documents.doc_next_rev, project_documents.status, project_documents.create_datetime, "
					+ "project_documents.edit_datetime, "
					+ "files.file_id, files.file_type, files.file_key, files.bucket_name, files.file_size, project_documents.submission_datetime, project_documents.project_doc_original_filename, "
					+ "project_documents.process_status, project_documents.doc_name_abbrv, project_documents.display_name, "
					+ "project_submissions.submission_name, project_submissions.submitter_email, "
					+ "project_documents.doc_desc, project_documents.doc_discipline, project_documents.doc_type, project_documents.submission_id, "
					+ "project_documents.doc_size "
					+ "FROM project_documents LEFT OUTER JOIN document_files ON document_files.doc_id=project_documents.doc_id "
					+ "LEFT OUTER JOIN files ON  files.file_id=document_files.file_id "
					+ "LEFT OUTER JOIN project_submissions ON project_submissions.project_submission_id=project_documents.submission_id "
					+ "WHERE project_documents.doc_id=@doc_id";
				cmd.Parameters.AddWithValue("@doc_id", docId);

				using (var reader = cmd.ExecuteReader())
				{
					var result = new Dictionary<string, string>();
					while (reader.Read())
					{
						result = new Dictionary<string, string>()
						{
							{ "doc_id", dbHelper.SafeGetString(reader, 0) },
							{ "project_id", dbHelper.SafeGetString(reader, 1) },
							{ "doc_number", dbHelper.SafeGetString(reader, 2) },
							{ "doc_name", dbHelper.SafeGetString(reader, 3) },
							{ "doc_version", dbHelper.SafeGetString(reader, 4) },
							{ "doc_revision", dbHelper.SafeGetString(reader, 5) },
							{ "doc_next_rev", dbHelper.SafeGetString(reader, 6) },
							{ "status", dbHelper.SafeGetString(reader, 7) },
							{ "create_datetime", dbHelper.SafeGetDatetimeString(reader, 8) },
							{ "edit_datetime", dbHelper.SafeGetDatetimeString(reader, 9) },
							{ "file_id", dbHelper.SafeGetString(reader, 10) },
							{ "file_type", dbHelper.SafeGetString(reader, 11) },
							{ "file_key", dbHelper.SafeGetString(reader, 12) },
							{ "bucket_name", dbHelper.SafeGetString(reader, 13) },
							{ "file_size", dbHelper.SafeGetString(reader, 14) },
							{ "submission_datetime", dbHelper.SafeGetDatetimeString(reader, 15) },
							{ "project_doc_original_filename", dbHelper.SafeGetString(reader, 16) },
							{ "process_status", dbHelper.SafeGetString(reader, 17) },
							{ "doc_name_abbrv", dbHelper.SafeGetString(reader, 18) },
							{ "display_name", dbHelper.SafeGetString(reader, 19) },
							{ "submission_name", dbHelper.SafeGetString(reader, 20) },
							{ "submitter_email", dbHelper.SafeGetString(reader, 21) },
							{ "doc_desc", dbHelper.SafeGetString(reader, 22) },
							{ "doc_discipline", dbHelper.SafeGetString(reader, 23) },
							{ "doc_type", dbHelper.SafeGetString(reader, 24) },
							{ "submission_id", dbHelper.SafeGetString(reader, 25) },
							{ "doc_size", dbHelper.SafeGetString(reader, 26) }
						};
					}
					reader.Close();

					return result;
				}
			}
		}

		public List<Dictionary<string, object>> RetrieveMatchedDocumentsWithKeyAttributes(
			DatabaseHelper dbHelper,
			string chainDocId,    // Document ID in the revision chain (next_rev or prev_rev)
			string docId,			// Document ID for the updated document
			string docNumber,
			string docPageNumber,
			string docSubProject)
		{
			var updatedDocFileOriginalModifiedDateTime = DateTime.UtcNow;

			if ((string.IsNullOrEmpty(chainDocId) && string.IsNullOrEmpty(docId))
				|| (!string.IsNullOrEmpty(chainDocId) && !string.IsNullOrEmpty(docId)))
			{
				throw new Exception("Invalid request on retrieving matched documents with key attributes");
			}

			// Get correct document key attributes
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT doc_number, doc_pagenumber, doc_subproject "
					+ "FROM project_documents WHERE doc_id = @doc_id";
				if (string.IsNullOrEmpty(chainDocId))
				{
					cmd.Parameters.AddWithValue("@doc_id", docId);
				}
				else
				{
					cmd.Parameters.AddWithValue("@doc_id", chainDocId);
				}
				
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						if (string.IsNullOrEmpty(chainDocId))
						{
							docNumber = docNumber ?? dbHelper.SafeGetString(reader, 0);
							docPageNumber = docPageNumber ?? dbHelper.SafeGetString(reader, 1);
							docSubProject = docSubProject ?? dbHelper.SafeGetString(reader, 2);
						}
						else
						{
							docNumber = dbHelper.SafeGetString(reader, 0);
							docPageNumber = dbHelper.SafeGetString(reader, 1);
							docSubProject = dbHelper.SafeGetString(reader, 2);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(docId))
						{
							throw new Exception($"Invalid document id: {docId}");
						}
						if (!string.IsNullOrEmpty(chainDocId))
						{
							throw new Exception($"Invalid document id: {chainDocId}");
						}
					}
				}
			}

			// Get matched project document with the key attributes
			using (var cmd = dbHelper.SpawnCommand())
			{
				string commandText = "SELECT "
					+ "project_documents.doc_id, project_documents.doc_revision, "
					+ "files.file_original_modified_datetime, files.bucket_name, files.file_original_filename, "
					+ "project_documents.submission_datetime, project_submissions.project_id, "
					+ "project_submissions.submitter_email, project_submissions.user_timezone, "
					+ "project_submissions.submission_name, project_submissions.project_submission_id, "
					+ "project_submissions.project_name "
					+ "FROM project_documents "
					+ "LEFT JOIN document_files ON document_files.doc_id = project_documents.doc_id "
					+ "LEFT JOIN files on files.file_id = document_files.file_id "
					+ "LEFT JOIN project_submissions ON project_submissions.project_submission_id = project_documents.submission_id "
					+ "WHERE project_documents.doc_id <> @doc_id "
					+ "AND files.file_type = 'source_system_original' ";
				commandText += string.IsNullOrEmpty(docNumber)
					? "AND project_documents.doc_number IS NULL "
					: $"AND project_documents.doc_number = '{docNumber}' ";
				commandText += string.IsNullOrEmpty(docPageNumber)
					? "AND project_documents.doc_pagenumber IS NULL "
					: $"AND project_documents.doc_pagenumber = '{docPageNumber}' ";
				commandText += string.IsNullOrEmpty(docSubProject)
					? "AND project_documents.doc_subproject IS NULL "
					: $"AND project_documents.doc_subproject = '{docSubProject}' ";

				cmd.CommandText = commandText;
				cmd.Parameters.AddWithValue("@doc_id", docId);

				var resultList = new List<Dictionary<string, object>>();
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add(new Dictionary<string, object>()
						{
							{ "doc_id", dbHelper.SafeGetString(reader, 0) },
							{ "doc_revision", dbHelper.SafeGetString(reader, 1) },
							{
								"file_original_modified_datetime",
								DateTimeHelper.ConvertToUTCDateTime(dbHelper.SafeGetDatetimeString(reader, 2))
							},
							{ "bucket_name", dbHelper.SafeGetString(reader, 3) },
							{ "file_original_filename", dbHelper.SafeGetString(reader, 4) },
							{ "submission_datetime", dbHelper.SafeGetDatetimeString(reader, 5) },
							{ "project_id", dbHelper.SafeGetString(reader, 6) },
							{ "submitter_email", dbHelper.SafeGetString(reader, 7) },
							{ "user_timezone", dbHelper.SafeGetString(reader, 8) },
							{ "submission_name", dbHelper.SafeGetString(reader, 9) },
							{ "submission_id", dbHelper.SafeGetString(reader, 10) },
							{ "project_name", dbHelper.SafeGetString(reader, 11) }
						});
					}
				}

				resultList.Sort((first, second) =>
					((DateTime)first["file_original_modified_datetime"]).CompareTo(
						(DateTime)second["file_original_modified_datetime"]));

				return resultList;
			}
		}
	}
}
