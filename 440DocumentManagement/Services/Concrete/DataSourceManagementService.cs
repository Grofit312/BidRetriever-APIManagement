using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSource;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class DataSourceManagementService : IDataSourceManagementService
	{
		private IBaseService _baseService;

		public DataSourceManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateDataSource(DatabaseHelper dbHelper, DataSourceModel newRecord)
		{
			newRecord.DataSourceStatus = newRecord.DataSourceStatus ?? "active";
			return _baseService.CreateRecord(newRecord, "data_sources", "data_source_id");
		}

		public List<Dictionary<string, object>> FindDataSources(DatabaseHelper dbHelper, DataSourceFindRequestModel request)
		{
			var extendedRequest = new DataSourceFindRequestExtendedModel()
			{
				CustomerId = new string[] { "default" },
				DataSourceStatus = request.DataSourceStatus ?? "active"
			};
			if (!string.IsNullOrEmpty(request.CustomerId))
			{
				extendedRequest.CustomerId = new string[] { "default", request.CustomerId };
			}

			using (var cmd = dbHelper.SpawnCommand())
			{
				var where = "# ";
				if (string.IsNullOrEmpty(request.CustomerId))
				{
					where += $"customer_id=@customer_id * ";
					cmd.Parameters.AddWithValue("@customer_id", "default");
				}
				else
				{
					where += $"(customer_id=@customer_id_1 $ customer_id=@customer_id_2) * ";
					cmd.Parameters.AddWithValue("@customer_id_1", request.CustomerId);
					cmd.Parameters.AddWithValue("@customer_id_2", "default");
				}
				where += $"data_source_status=@data_source_status * ";
				cmd.Parameters.AddWithValue("@data_source_status", request.DataSourceStatus ?? "active");
				where = where.Remove(where.Length - 2);
				where = where.Replace("# ", " WHERE ").Replace("* ", "AND ").Replace("$ ", "OR ");

				cmd.CommandText = "SELECT data_sources.create_datetime, data_sources.edit_datetime, "
					+ "data_sources.create_user_id, data_sources.edit_user_id, "
					+ "data_sources.data_source_status, data_sources.data_source_id, data_sources.data_source_name, "
					+ "data_sources.data_source_desc, data_sources.data_source_base_query, "
					+ "data_source_fields.data_source_field_id, data_source_fields.data_source_field_name, "
					+ "data_source_fields.data_source_field_displayname, "
					+ "data_source_fields.data_source_field_desc, "
					+ "data_source_fields.data_source_field_datatype, data_source_fields.required_field, "
					+ "data_source_fields.data_source_field_heading, data_source_fields.data_source_field_status "
					+ "FROM data_sources "
					+ "LEFT JOIN data_source_fields ON data_sources.data_source_id = data_source_fields.data_source_id "
					+ where;

				var resultList = new List<Dictionary<string, object>>();
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var dataSourceId = Convert.ToString(reader["data_source_id"]);
						var existedIndex = resultList.FindIndex(item => item["data_source_id"].ToString() == dataSourceId);
						if (existedIndex >= 0)
						{
							((List<Dictionary<string, string>>)resultList[existedIndex]["data_source_fields"]).Add(
								new Dictionary<string, string>()
								{
									{ "data_source_field_id", Convert.ToString(reader["data_source_field_id"]) },
									{ "data_source_field_name", Convert.ToString(reader["data_source_field_name"]) },
									{ "data_source_field_displayname", Convert.ToString(reader["data_source_field_displayname"]) },
									{ "data_source_field_desc", Convert.ToString(reader["data_source_field_desc"]) },
									{ "data_source_field_datatype", Convert.ToString(reader["data_source_field_datatype"]) },
									{ "required_field", Convert.ToString(reader["required_field"]) },
									{ "data_source_field_heading", Convert.ToString(reader["data_source_field_heading"]) },
									{ "data_source_field_status", Convert.ToString(reader["data_source_field_status"]) }
								});
						}
						else
						{
							var dataFieldsDict = new List<Dictionary<string, string>>();
							dataFieldsDict.Add(new Dictionary<string, string>()
							{
								{ "data_source_field_id", Convert.ToString(reader["data_source_field_id"]) },
								{ "data_source_field_name", Convert.ToString(reader["data_source_field_name"]) },
								{ "data_source_field_desc", Convert.ToString(reader["data_source_field_desc"]) },
								{ "data_source_field_datatype", Convert.ToString(reader["data_source_field_datatype"]) },
								{ "required_field", Convert.ToString(reader["required_field"]) },
								{ "data_source_field_heading", Convert.ToString(reader["data_source_field_heading"]) },
								{ "data_source_field_status", Convert.ToString(reader["data_source_field_status"]) }
							});

							resultList.Add(new Dictionary<string, object>
							{
								{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
								{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
								{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
								{ "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
								{ "data_source_desc", Convert.ToString(reader["data_source_desc"]) },
								{ "data_source_id", Convert.ToString(reader["data_source_id"]) },
								{ "data_source_name", reader["data_source_name"] },
								{ "data_source_base_query", Convert.ToString(reader["data_source_base_query"]) },
								{ "data_source_status", Convert.ToString(reader["data_source_status"]) },
								{ "data_source_fields", dataFieldsDict }
							});
						}
					}
				}
				return resultList;
			}
		}

		public Dictionary<string, object> GetDataSource(DatabaseHelper dbHelper, DataSourceGetRequestModel request)
		{
			try
			{
				using (var cmd = dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT data_sources.create_datetime, data_sources.edit_datetime, "
						+ "data_sources.create_user_id, data_sources.edit_user_id, "
						+ "data_sources.data_source_status, data_sources.data_source_id, data_sources.data_source_name, "
						+ "data_sources.data_source_desc, data_sources.data_source_base_query, "
						+ "data_source_fields.data_source_field_id, data_source_fields.data_source_field_name, "
						+ "data_source_fields.data_source_field_displayname "
						+ "data_source_fields.data_source_field_desc, "
						+ "data_source_fields.data_source_field_datatype, data_source_fields.required_field, "
						+ "data_source_fields.data_source_field_heading, data_source_fields.data_source_field_status "
						+ "FROM data_sources "
						+ "LEFT JOIN data_source_fields ON data_sources.data_source_id = data_source_fields.data_source_id "
						+ "WHERE data_sources.data_source_id=@data_source_id";
					cmd.Parameters.AddWithValue("@data_source_id", request.DataSourceId);

					Dictionary<string, object> result = null;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (result == null)
							{
								result = new Dictionary<string, object>
								{
									{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
									{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
									{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
									{ "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
									{ "data_source_desc", Convert.ToString(reader["data_source_desc"]) },
									{ "data_source_id", Convert.ToString(reader["data_source_id"]) },
									{ "data_source_name", reader["data_source_name"] },
									{ "data_source_base_query", Convert.ToString(reader["data_source_base_query"]) },
									{ "data_source_status", Convert.ToString(reader["data_source_status"]) },
									{ "data_source_fields", new List<Dictionary<string, object>>() }
								};
							}

							var dataSourceFields = new Dictionary<string, object>()
							{
								{ "data_source_field_id", Convert.ToString(reader["data_source_field_id"]) },
								{ "data_source_field_name", Convert.ToString(reader["data_source_field_name"]) },
								{ "data_source_field_displayname", Convert.ToString(reader["data_source_field_displayname"]) },
								{ "data_source_field_desc", Convert.ToString(reader["data_source_field_desc"]) },
								{ "data_source_field_datatype", Convert.ToString(reader["data_source_field_datatype"]) },
								{ "required_field", Convert.ToString(reader["required_field"]) },
								{ "data_source_field_heading", Convert.ToString(reader["data_source_field_heading"]) },
								{ "data_source_field_status", Convert.ToString(reader["data_source_field_status"]) }
							};
							(result["data_source_fields"] as List<Dictionary<string, object>>).Add(dataSourceFields);
						}
					}
					return result;
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public int UpdateDataSource(DatabaseHelper dbHelper, DataSourceUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "data_sources");
		}
	}
}
