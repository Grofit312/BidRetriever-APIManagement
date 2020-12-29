using _440DocumentManagement.Helpers;
using _440DocumentManagement.Services.Interface;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
			List<Dictionary<string, string>> documents)
		{
			var isCounterBasedRevision = false;
            var timezone = "eastern";

            using (var cmd = dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT customer_timezone FROM customers "
                    + $"WHERE customer_id = '{customerId}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        timezone = dbHelper.SafeGetString(reader, 0);
                    }
                }
            }

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
					updatedDocRevision = "";
				}
				else
				{
					updatedDocRevision = (Int32.Parse((string)documents.Last()["doc_revision"]) + 1).ToString().PadLeft(2, '0');
				}
			}
			else
			{
                updatedDocRevision = CalculateDocRevisionForSubmissionDate((string)currentDoc["submission_datetime"], timezone, documents);
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
				cmd.CommandText = "SELECT project_documents.doc_id, doc_revision, files.file_key, files.bucket_name, "
                        + "files.file_original_modified_datetime, files.parent_original_modified_datetime, project_documents.create_datetime, "
                        + "project_documents.submission_datetime "
                        + "FROM project_documents "
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
                            ["file_original_modified_datetime"] = dbHelper.SafeGetDatetimeString(reader, 4),
                            ["parent_original_modified_datetime"] = dbHelper.SafeGetDatetimeString(reader, 5),
                            ["create_datetime"] = dbHelper.SafeGetDatetimeString(reader, 6),
                            ["submission_datetime"] = dbHelper.SafeGetDatetimeString(reader, 7),
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

					return result;
				}
			}
		}

		public List<Dictionary<string, string>> RetrieveMatchedDocumentsWithKeyAttributes(
			DatabaseHelper dbHelper,
			string docId,			// Document ID for the updated document
            string projectId,       // Project ID that document belongs to
			string docNumber,
			string docPageNumber,
			string docSubProject)
		{
			if (string.IsNullOrEmpty(docId))
			{
				throw new Exception("Invalid request on retrieving matched documents with key attributes");
			}

			// Get correct document key attributes
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT doc_number, doc_pagenumber, doc_subproject "
					+ "FROM project_documents WHERE doc_id = @doc_id";

				cmd.Parameters.AddWithValue("@doc_id", docId);
				
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						docNumber = docNumber ?? dbHelper.SafeGetString(reader, 0);
						docPageNumber = docPageNumber ?? dbHelper.SafeGetString(reader, 1);
						docSubProject = docSubProject ?? dbHelper.SafeGetString(reader, 2);
					}
					else
					{
						throw new Exception($"Invalid document id: {docId}");
					}
				}
			}

			// Get matched project document with the key attributes
			var matchedDocId = "";

			using (var cmd = dbHelper.SpawnCommand())
			{
                string commandText = $"SELECT doc_id FROM project_documents WHERE project_id='{projectId}' ";
				commandText += string.IsNullOrEmpty(docNumber)
					? "AND (doc_number IS NULL OR doc_number='') "
					: $"AND doc_number = '{docNumber}' ";
				commandText += string.IsNullOrEmpty(docPageNumber)
					? "AND (doc_pagenumber IS NULL OR doc_pagenumber='') "
                    : $"AND doc_pagenumber = '{docPageNumber}' ";
				commandText += string.IsNullOrEmpty(docSubProject)
					? "AND (doc_subproject IS NULL OR doc_subproject='') "
                    : $"AND doc_subproject = '{docSubProject}' ";

				cmd.CommandText = commandText;

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
                        matchedDocId = reader["doc_id"] as string;
					}
                    else
                    {
                        return new List<Dictionary<string, string>> { };
                    }
				}
			}

			var revisionChain = GetDocumentRevisions(dbHelper, matchedDocId);

			return revisionChain;
		}

        public List<Dictionary<string, object>> FindFolderTransactionLogs(
            DatabaseHelper databaseHelper,
            string doc_id)
        {
            var logs = new List<Dictionary<string, object>> { };

            using (var cmd = databaseHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT * FROM folder_transaction_log LEFT JOIN project_folders ON project_folders.folder_id=folder_transaction_log.folder_id"
						+ $" WHERE doc_id='{doc_id}' AND folder_type NOT LIKE 'source%' ORDER BY folder_transaction_sequence_num ASC";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) {
                        var projectId = reader["project_id"] as string;
                        var folderId = reader["folder_id"] as string;
                        var operationType = reader["operation_type"] as string;
                        var fileId = reader["file_id"] as string;
                        var originalFolderName = reader["original_folder_name"] as string;
                        var newFolderName = reader["new_folder_name"] as string;
                        var folderPath = reader["folder_path"] as string;

                        var matchedLog = logs.Find(log =>
                        {
                            return log["folder_id"] as string == folderId && log["file_id"] as string == fileId;
                        });

                        if (operationType == "remove_file" && matchedLog != null)
                        {
                            logs.Remove(matchedLog);
                        }
                        else if (operationType == "add_file" && matchedLog == null)
                        {
                            logs.Add(new Dictionary<string, object>
                            {
                                { "project_id", projectId },
                                { "folder_id", folderId },
                                { "operation_type", operationType },
                                { "file_id", fileId },
                                { "original_folder_name", originalFolderName },
                                { "new_folder_name", newFolderName },
                                { "folder_path", folderPath },
                                { "doc_id", doc_id },
                            });
                        }
                    }
                }
            }

            return logs;
        }

        public Dictionary<string, string> GetInfoForKeyAttributeUpdate(DatabaseHelper dbHelper, string docId)
        {
            var info = new Dictionary<string, string> { };

            using (var cmd = dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT project_documents.project_id, project_documents.create_datetime,  "
                    + "files.file_original_modified_datetime, files.parent_original_modified_datetime, "
                    + "project_documents.submission_datetime, customers.customer_timezone, projects.project_name, "
					+ "project_submissions.submitter_email, project_documents.submission_id, project_submissions.submission_name, "
					+ "project_submissions.submission_type "
                    + "FROM project_documents "
                    + "LEFT JOIN projects ON projects.project_id=project_documents.project_id "
					+ "LEFT JOIN project_submissions ON project_submissions.project_submission_id=project_documents.submission_id "
                    + "LEFT JOIN customers ON projects.project_customer_id=customers.customer_id "
					+ "LEFT JOIN document_files ON project_documents.doc_id=document_files.doc_id "
					+ "LEFT JOIN files ON files.file_id=document_files.file_id "
                    + $"WHERE project_documents.doc_id='{docId}' AND files.file_type='source_system_original'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        info["project_id"] = dbHelper.SafeGetString(reader, 0);
                        info["create_datetime"] = dbHelper.SafeGetDatetimeString(reader, 1);
                        info["file_original_modified_datetime"] = dbHelper.SafeGetDatetimeString(reader, 2);
                        info["parent_original_modified_datetime"] = dbHelper.SafeGetDatetimeString(reader, 3);
                        info["submission_datetime"] = dbHelper.SafeGetDatetimeString(reader, 4);
                        info["customer_timezone"] = dbHelper.SafeGetString(reader, 5);
						info["project_name"] = dbHelper.SafeGetString(reader, 6);
						info["submitter_email"] = dbHelper.SafeGetString(reader, 7);
						info["submission_id"] = dbHelper.SafeGetString(reader, 8);
						info["submission_name"] = dbHelper.SafeGetString(reader, 9);
						info["submission_type"] = dbHelper.SafeGetString(reader, 10);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            var revisioningType = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_REVISIONING_TYPE");
            info["revisioning_type"] = string.IsNullOrEmpty(revisioningType) ? "Submission Date" : revisioningType;

			var disciplinePlansFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_DISCIPLINE_PLANS_FOLDER");
			info["discipline_plans_folder"] = string.IsNullOrEmpty(disciplinePlansFolder) ? "disabled" : disciplinePlansFolder;

			var planFileNaming = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_PLAN_FILE_NAMING");
			info["plan_file_naming"] = string.IsNullOrEmpty(planFileNaming) ? "<doc_num>__<doc_name>__<doc_revision>" : planFileNaming;

			var currentPlansFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_CURRENT_PLANS_FOLDER");
			info["current_plans_folder"] = string.IsNullOrEmpty(currentPlansFolder) ? "enabled" : currentPlansFolder;

			var allPlansFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_ALL_PLANS_FOLDER");
			info["all_plans_folder"] = string.IsNullOrEmpty(allPlansFolder) ? "enabled" : allPlansFolder;

			var allPlansSubmissionFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_ALL_PLANS_SUBMISSION_FOLDER");
			info["all_plans_submission_folder"] = string.IsNullOrEmpty(allPlansSubmissionFolder) ? "disabled" : allPlansSubmissionFolder;

			var comparisonPlansFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_COMPARISON_PLANS_FOLDER");
			info["comparison_plans_folder"] = string.IsNullOrEmpty(comparisonPlansFolder) ? "separate_comparison_folder" : comparisonPlansFolder;

			var rasterPlansFolder = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_RASTER_PLANS_FOLDER");
			info["raster_plans_folder"] = string.IsNullOrEmpty(rasterPlansFolder) ? "disabled" : rasterPlansFolder;

			info["destination_root_path"] = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_DESTINATION_PATH");
			info["destination_type"] = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_DESTINATION_TYPE_ID");
			info["destination_token"] = GetProjectSetting(dbHelper, info["project_id"], "PROJECT_DESTINATION_TOKEN");

			var docFile = GetSourceFileInfo(dbHelper, docId);

			if (docFile == null)
            {
				return null;
            }

			info["file_id"] = docFile["file_id"];
			info["file_original_filename"] = docFile["file_original_filename"];

            return info;
        }

        public string GetProjectSetting(DatabaseHelper dbHelper, string project_id, string setting_id)
        {
            using (var cmd = dbHelper.SpawnCommand())
            {
                cmd.CommandText = "SELECT setting_value FROM project_settings "
                    + $"WHERE project_id='{project_id}' AND setting_name='{setting_id}'";

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return dbHelper.SafeGetString(reader, 0);
                    }
                }
            }

            return null;
        }

		public string GetSystemSetting(DatabaseHelper dbHelper, string setting_name)
		{
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT setting_value FROM system_settings "
					+ $"WHERE setting_name='{setting_name}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return dbHelper.SafeGetString(reader, 0);
					}
				}
			}

			return null;
		}

		public string CalculateDocRevisionForSubmissionDate(string submission_datetime, string timezone, List<Dictionary<string, string>> documents)
        {
            var docRevision = "";
            var submissionDatetime = DateTimeHelper.ConvertToUserTimezone(submission_datetime, timezone);
            var timestampWithoutHM = submissionDatetime.ToString("yyyy-MM-dd");
            var timestampWithHM = submissionDatetime.ToString("yyyy-MM-dd_HH-mm");

            if (documents.Count == 0)
            {
                docRevision = "NULL";
            }
            else
            {
                var numberOfPreviousDocsInSameDay = documents.Where(prevDoc =>
                {
                    var prevDocSubmissionDatetime = DateTimeHelper.ConvertToUserTimezone((string)prevDoc["submission_datetime"], timezone);
                    return prevDocSubmissionDatetime.ToString("yyyy-MM-dd") == timestampWithoutHM;
                }).ToList().Count;
                if (numberOfPreviousDocsInSameDay == 0)
                {
                    docRevision = timestampWithoutHM;
                }
                else
                {
                    docRevision = timestampWithoutHM;
                    docRevision += " - " + numberOfPreviousDocsInSameDay.ToString("D2");
                }
            }

            return docRevision;
        }

		public Dictionary<string, string> GetCurrentPlanFolderContentRecord(DatabaseHelper dbHelper, string docId)
        {
			using (var cmd = dbHelper.SpawnCommand())
            {
				cmd.CommandText = "SELECT project_folder_contents.folder_content_id, project_folder_contents.folder_id, project_folder_contents.folder_path FROM project_folder_contents "
					+ "LEFT JOIN project_folders ON project_folders.folder_id=project_folder_contents.folder_id "
					+ $"WHERE project_folder_contents.doc_id='{docId}' AND project_folder_contents.status='active' AND project_folders.folder_type='plans_current'";

				using (var reader = cmd.ExecuteReader())
                {
					if (reader.Read())
                    {
						return new Dictionary<string, string>
						{
							{ "folder_content_id", dbHelper.SafeGetString(reader, 0) },
							{ "folder_id", dbHelper.SafeGetString(reader, 1) },
							{ "folder_path", dbHelper.SafeGetString(reader, 2) },
						};
                    }
                }
            }

			return null;
        }

		public List<Dictionary<string, string>> GetAttachedFiles(DatabaseHelper dbHelper, string docId)
        {
			var result = new List<Dictionary<string, string>> { };

			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT files.file_id, files.file_original_filename, files.file_type, files.file_size FROM project_documents "
					+ "LEFT JOIN document_files ON document_files.doc_id=project_documents.doc_id "
					+ "LEFT JOIN files ON files.file_id=document_files.file_id "
					+ $"WHERE project_documents.doc_id='{docId}' ORDER BY files.create_datetime DESC";

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var fileInfo = new Dictionary<string, string>
						{
							{ "file_id", dbHelper.SafeGetString(reader, 0) },
							{ "file_original_filename", dbHelper.SafeGetString(reader, 1) },
							{ "file_type", dbHelper.SafeGetString(reader, 2) },
							{ "file_size", dbHelper.SafeGetString(reader, 3) },
						};

						result.Add(fileInfo);
					}
				}
			}

			return result;
		}

		public Dictionary<string, string> GetSourceFileInfo(DatabaseHelper dbHelper, string docId)
        {
			var files = GetAttachedFiles(dbHelper, docId);
			var enhancedFile = files.Find(file =>
			{
				return file["file_type"] == "enhanced_original";
			});

			if (enhancedFile != null)
            {
				return enhancedFile;
            }

			var sourceFile = files.Find(file =>
			{
				return file["file_type"] == "source_system_original";
			});

			return sourceFile;
        }

		public string GetDisciplineFolderName(string docNumber)
        {
			if (docNumber.StartsWith('A'))
			{
				return "Architectural";
			}
			else if (docNumber.StartsWith('C'))
			{
				return "Civil";
			}
			else if (docNumber.StartsWith('D'))
			{
				return "Demolition";
			}
			else if (docNumber.StartsWith('E'))
			{
				return "Electrical";
			}
			else if (docNumber.StartsWith("FA") || docNumber.StartsWith("FP"))
			{
				return "Fire Protection";
			}
			else if (docNumber.StartsWith('G'))
			{
				return "General";
			}
			else if (docNumber.StartsWith('H'))
			{
				return "HVAC";
			}
			else if (docNumber.StartsWith('I'))
			{
				return "Interior";
			}
			else if (docNumber.StartsWith('L') || docNumber.StartsWith("LA"))
			{
				return "Landscape";
			}
			else if (docNumber.StartsWith("LS"))
			{
				return "Life Safety";
			}
			else if (docNumber.StartsWith('M'))
			{
				return "Mechanical";
			}
			else if (docNumber.StartsWith('P'))
			{
				return "Plumbing";
			}
			else if (docNumber.StartsWith('Q') || docNumber.StartsWith("EQ"))
			{
				return "Equipment";
			}
			else if (docNumber.StartsWith('S'))
			{
				return "Structural";
			}
			else if (docNumber.StartsWith('T'))
			{
				return "Telecommunications";
			}
			else
			{
				return "Other";
			}
		}

		public async Task CreateComparison(DatabaseHelper dbHelper, string currentDocId, string prevDocId, Dictionary<string, string> relatedInfo)
        {
			var wipApiEndpoint = GetSystemSetting(dbHelper, "BR_WIPAPI_ENDPOINT");
			var vaultBucket = GetSystemSetting(dbHelper, "BR_PERM_VAULT");

			if (string.IsNullOrEmpty(wipApiEndpoint))
            {
				// wip api endpoint not available, should terminate
				return;
            }

			using (var client = new HttpClient())
            {
				client.BaseAddress = new Uri(wipApiEndpoint);
				client.Timeout = TimeSpan.FromSeconds(60);

				try
				{
					var data = new Dictionary<string, string>
					{
						{ "doc_id", currentDocId },
						{ "prev_doc_id", prevDocId },
						{ "file_original_filename", relatedInfo["file_original_filename"] },
						{ "project_id", relatedInfo["project_id"] },
						{ "project_name", relatedInfo["project_name"] },
						{ "vault_bucket", vaultBucket },
						{ "submitter_email", relatedInfo["submitter_email"] },
						{ "submission_datetime", relatedInfo["submission_datetime"] },
						{ "submission_id", relatedInfo["submission_id"] },
						{ "user_timezone", relatedInfo["customer_timezone"] },
					};
					var request = new HttpRequestMessage(HttpMethod.Post, "Create9414")
					{
						Content = new FormUrlEncodedContent(data)
					};

					await client.SendAsync(request);
				}
				catch (Exception exception)
				{
					// Do nothing
					var exceptionMessage = exception.Message;
				}
			}
		}

		public void RecreateFolderTransactionLog(DatabaseHelper dbHelper, string docId)
        {
			var existingLogs = FindFolderTransactionLogs(dbHelper, docId);

			foreach (var log in existingLogs)
			{
				CreateFolderTransactionLog(dbHelper, log, "remove_file");
			}

			foreach (var log in existingLogs)
			{
				CreateFolderTransactionLog(dbHelper, log, "add_file");
			}
		}

		private string GetPlanFileName(string docNumber, string docName, string docSubproject, string docPagenumber, string docRevision, string planFileNamingTemplate)
        {
			if (!string.IsNullOrEmpty(docSubproject))
            {
				docNumber = docSubproject + "_" + docNumber;
            }
			if (!string.IsNullOrEmpty(docPagenumber))
            {
				docNumber = docNumber + "_" + docPagenumber;
            }

			var mergedPlanFileName = planFileNamingTemplate;

			if (mergedPlanFileName.Contains("<doc_num>"))
            {
				mergedPlanFileName = mergedPlanFileName.Replace("<doc_num>", docNumber ?? "");
            }

			if (mergedPlanFileName.Contains("<doc_name>"))
            {
				mergedPlanFileName = mergedPlanFileName.Replace("__<doc_name>", string.IsNullOrEmpty(docName) ? "" : $"__{docName}");
            }

			if (mergedPlanFileName.Contains("<doc_revision>"))
            {
				mergedPlanFileName = mergedPlanFileName.Replace("__<doc_revision>", string.IsNullOrEmpty(docRevision) ? "" : $"__{docRevision}");
            }

			mergedPlanFileName = mergedPlanFileName.Replace("__", "_");

			return mergedPlanFileName;
        }

		public Dictionary<string, string> GetDocumentInfo(DatabaseHelper dbHelper, string docId)
        {
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "SELECT doc_name, doc_number, doc_revision, doc_subproject, doc_pagenumber FROM project_documents "
					+ $"WHERE doc_id='{docId}'";

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return new Dictionary<string, string>
						{
							{ "doc_name", dbHelper.SafeGetString(reader, "doc_name") },
							{ "doc_number", dbHelper.SafeGetString(reader, "doc_number") },
							{ "doc_revision", dbHelper.SafeGetString(reader, "doc_revision") },
							{ "doc_subproject", dbHelper.SafeGetString(reader, "doc_subproject") },
							{ "doc_pagenumber", dbHelper.SafeGetString(reader, "doc_pagenumber") },
						};
					}
					else
					{
						return null;
					}
				}
			}
		}

		private List<Dictionary<string, string>> GetPublishedPlanRecords(DatabaseHelper dbHelper, string docId)
        {
			using (var cmd = dbHelper.SpawnCommand())
            {
				var records = new List<Dictionary<string, string>>();

				cmd.CommandText = "SELECT destination_file_name, destination_folder_path, doc_publish_id "
					+ "FROM project_documents_published "
					+ $"WHERE doc_id='{docId}' AND status='active' AND publish_status='completed' AND destination_folder_path LIKE 'Plans%' "
					+ "ORDER BY publish_datetime DESC";

				using (var reader = cmd.ExecuteReader())
                {
					while (reader.Read())
                    {
						var destinationFileName = dbHelper.SafeGetString(reader, "destination_file_name");
						var destinationFolderPath = dbHelper.SafeGetString(reader, "destination_folder_path");
						var docPublishId = dbHelper.SafeGetString(reader, "doc_publish_id");

						var existing = records.Find(record => record["destination_folder_path"] == destinationFolderPath);

						if (existing == null)
                        {
							records.Add(new Dictionary<string, string>
							{
								{ "destination_file_name", destinationFileName },
								{ "destination_folder_path", destinationFolderPath },
								{ "doc_publish_id", docPublishId },
							});
                        }
					}
                }

				return records;
            }
        }

		private void UpdatePublishedRecord(DatabaseHelper dbHelper, string docPublishId, string destinationFileName = null, string status = null)
        {
			using (var cmd = dbHelper.SpawnCommand())
            {
				cmd.CommandText = "UPDATE project_documents_published "
					+ (!string.IsNullOrEmpty(destinationFileName) ? $"SET destination_file_name='{destinationFileName}' " : "")
					+ (!string.IsNullOrEmpty(status) ? $"SET status='{status}' " : "")
					+ $"WHERE doc_publish_id='{docPublishId}'";

				cmd.ExecuteNonQuery();
            }
        }

		public async Task<bool> RepublishDocuments(DatabaseHelper dbHelper, List<string> docIds, Dictionary<string, string> relatedInfo)
        {
			try
            {
				var dbx = new DropboxHelper(relatedInfo["destination_token"]);
				var tempMovedList = new List<Dictionary<string, string>> { };

				foreach (var docId in docIds)
                {
					var document = GetDocumentInfo(dbHelper, docId);

					if (document == null)
					{
						return false;
					}

					var newFileName = GetPlanFileName(document["doc_number"], document["doc_name"], document["doc_subproject"], document["doc_pagenumber"], document["doc_revision"], relatedInfo["plan_file_naming"]);

					var publishedRecords = GetPublishedPlanRecords(dbHelper, docId);

					foreach (var publishedRecord in publishedRecords)
					{
						var extensionIndex = publishedRecord["destination_file_name"].LastIndexOf(".");
						var fileNamePart = publishedRecord["destination_file_name"].Substring(0, extensionIndex);
						var newFullFileName = publishedRecord["destination_file_name"].Replace(fileNamePart, newFileName);
						var originFullPath = $"{relatedInfo["destination_root_path"]}/{publishedRecord["destination_folder_path"]}/{publishedRecord["destination_file_name"]}";
						var newFullPath = $"{relatedInfo["destination_root_path"]}/{publishedRecord["destination_folder_path"]}/{newFullFileName}";

						UpdatePublishedRecord(dbHelper, publishedRecord["doc_publish_id"], newFullFileName);
						var moveResult = await dbx.MoveFile(originFullPath, newFullPath);

						if (!moveResult)
                        {
							var tempPath = "temp_" + newFullPath;
							await dbx.MoveFile(originFullPath, tempPath);
							tempMovedList.Add(new Dictionary<string, string>
							{
								{ "temp_path", tempPath },
								{ "target_path", newFullPath },
							});
                        }
					}
				}

				foreach (var temp in tempMovedList)
                {
					await dbx.MoveFile(temp["temp_path"], temp["target_path"]);
                }

				return true;
			}
			catch (Exception)
            {
				// Something went wrong
				return false;
            }
        }

		public async Task<bool> PublishDocumentToCurrentPlan(DatabaseHelper dbHelper, string docId, Dictionary<string, string> relatedInfo)
        {
			try
            {
				var wipApiEndpoint = GetSystemSetting(dbHelper, "BR_WIPAPI_ENDPOINT");
				var vaultBucket = GetSystemSetting(dbHelper, "BR_PERM_VAULT");

				if (string.IsNullOrEmpty(wipApiEndpoint))
				{
					// wip api endpoint not available, should terminate
					return false;
				}

				var docInfo = GetDocumentInfo(dbHelper, docId);

				if (docId == null)
                {
					// Document info not found
					return false;
                }

				var sourceFileInfo = GetSourceFileInfo(dbHelper, docId);

				if (sourceFileInfo == null)
                {
					// Source File not found
					return false;
                }

				// Create 964 record
				var destinationFileName = GetPlanFileName(docInfo["doc_number"], docInfo["doc_name"], docInfo["doc_subproject"], docInfo["doc_pagenumber"], docInfo["doc_revision"], relatedInfo["plan_file_naming"]) + ".pdf";
				var destinationPath = relatedInfo["discipline_plans_folder"] != "disabled"
					? $"Plans-Current/{GetDisciplineFolderName(docInfo["doc_number"])}"
					: "Plans-Current";

				var data = new Dictionary<string, string>
				{
					{ "doc_id", docId },
					{ "file_original_filename", sourceFileInfo["file_original_filename"] },
					{ "process_status", "queued" },
					{ "process_attempts", "0" },
					{ "project_id", relatedInfo["project_id"] },
					{ "project_name", relatedInfo["project_name"] },
					{ "publish_datetime", relatedInfo["submission_datetime"] },
					{ "submission_id", relatedInfo["submission_id"] },
					{ "submission_name", relatedInfo["submission_name"] },
					{ "submission_type", relatedInfo["submission_type"] },
					{ "submitter_email", relatedInfo["submitter_email"] },
					{ "user_timezone", relatedInfo["customer_timezone"] },
					{ "destination_filename", destinationFileName },
					{ "destination_path", destinationPath },
					{ "file_id", sourceFileInfo["file_id"] },
					{ "file_size", sourceFileInfo["file_size"] },
					{ "vault_bucket", vaultBucket },
				};

				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri(wipApiEndpoint);
					client.Timeout = TimeSpan.FromSeconds(60);

					var request = new HttpRequestMessage(HttpMethod.Post, "Create964")
					{
						Content = new FormUrlEncodedContent(data)
					};

					await client.SendAsync(request);
				}

				return true;
            }
			catch (Exception)
            {
				return false;
            }
        }

		public async Task<bool> UnpublishDocumentFromCurrentPlan(DatabaseHelper dbHelper, string docId, Dictionary<string, string> relatedInfo)
        {
			try
			{
				var publishedRecords = GetPublishedPlanRecords(dbHelper, docId);
				var currentPlanRecord = publishedRecords.Find(record => record["destination_folder_path"].Contains("Plans-Current"));

				if (currentPlanRecord != null)
                {
					var dbx = new DropboxHelper(relatedInfo["destination_token"]);

					await dbx.DeleteFile($"{relatedInfo["destination_root_path"]}/{currentPlanRecord["destination_folder_path"]}/{currentPlanRecord["destination_file_name"]}");

					UpdatePublishedRecord(dbHelper, currentPlanRecord["doc_publish_id"], null, "inactive");
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
