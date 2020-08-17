using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.ApiDatabase;
using _440DocumentManagement.Models.DataView;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace _440DocumentManagement.Services.Concrete
{
	public class DataViewManagementService : IDataViewManagementService
	{
		private readonly IBaseService _baseService;

		public DataViewManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateDataView(DatabaseHelper dbHelper, DataViewModel newRecord)
		{
			newRecord.ViewStatus = newRecord.ViewStatus ?? "active";
			string data_source_base_query = "", data_view_filter_sql = "";

			//------------------------------------------------------
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "select data_source_base_query from data_sources where data_source_id = @data_source_id";
				cmd.Parameters.AddWithValue("@data_source_id", newRecord.DataSourceId);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						data_source_base_query = Convert.ToString(reader["data_source_base_query"]);
					}
				}
			}
			using (var cmd = dbHelper.SpawnCommand())
			{
				cmd.CommandText = "select data_view_filter_sql from data_view_filter where data_view_filter_id = @data_view_filter_id";
				cmd.Parameters.AddWithValue("@data_view_filter_id", newRecord.DataFilterId);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						data_view_filter_sql = Convert.ToString(reader["data_view_filter_sql"]);
					}
				}
			}

			//------------------------------------------------------
			newRecord.ViewQueryGenerated = !string.IsNullOrEmpty(newRecord.ViewFieldList)
				? data_source_base_query.Replace("{}", " " + newRecord.ViewFieldList + " ") + " " + data_view_filter_sql
				: data_source_base_query + " " + data_view_filter_sql;
			return _baseService.CreateRecord(newRecord, "data_views", "view_id");
		}

		public List<object> FindDataViews(DatabaseHelper dbHelper, DataViewFindRequestModel request)
		{
			request.DetailLevel = request.DetailLevel ?? "basic";
			request.ViewStatus = request.ViewStatus ?? "active";

			try
			{
				string query = string.Format("SELECT "
					+ "data_views.company_id as customer_id, "
					+ "data_views.create_datetime, "
					+ "data_views.edit_datetime, "
					+ "data_views.group_id, "
					+ "data_views.office_id, "
					+ "data_views.user_id, "
					+ "users.user_displayname, "
					+ "data_sources.data_source_name as view_data_source_name, "
					+ "data_views.view_desc, "
					+ "data_view_filter.data_view_filter_name as view_filter_name, "
					+ "data_views.view_id, "
					+ "data_views.view_name, "
					+ "data_views.view_type, "
					+ "data_views.create_user_id, "
					+ "data_views.edit_user_id, "
					+ "company_offices.company_office_name as office_name, "
					+ "customers.customer_name as company_name "
					+ "FROM data_views "
					+ "LEFT JOIN data_view_filter ON data_views.data_filter_id = data_view_filter.data_view_filter_id "
					+ "LEFT JOIN data_sources ON data_views.data_source_id = data_sources.data_source_id "
	        + "LEFT JOIN users ON data_views.user_id = users.user_id "
	        + "LEFT JOIN company_offices ON data_views.office_id = company_offices.company_office_id "
	        + "LEFT JOIN customers ON data_views.company_id = customers.customer_id");

				string where = "# ";
				using (var cmd = dbHelper.SpawnCommand())
				{
					where += "data_views.view_status=@view_status * ";
					cmd.Parameters.AddWithValue("@view_status", request.ViewStatus);
					where += "data_views.view_type=@view_type * ";
					cmd.Parameters.AddWithValue("@view_type", request.ViewType);
					where += "(";
					if (string.IsNullOrEmpty(request.CustomerId))
					{
						where += "data_views.company_id=@company_id $ ";
						cmd.Parameters.AddWithValue("@company_id", "default");
					}
					else
					{
						where += "(data_views.company_id=@company_id_1 $ data_views.company_id=@company_id_2) $ ";
						cmd.Parameters.AddWithValue("@company_id_1", request.CustomerId);
						cmd.Parameters.AddWithValue("@company_id_2", "default");
					}
					if (!string.IsNullOrEmpty(request.GroupId))
					{
						where += "data_views.group_id = @group_id $ ";
						cmd.Parameters.AddWithValue("@group_id", request.GroupId);
					}
					if (!string.IsNullOrEmpty(request.OfficeId))
					{
						where += "data_views.office_id = @office_id $ ";
						cmd.Parameters.AddWithValue("@office_id", request.OfficeId);
					}
					if (!string.IsNullOrEmpty(request.UserId))
					{
						where += " data_views.user_id = @user_id $ ";
						cmd.Parameters.AddWithValue("@user_id", request.UserId);
					}

					where = where.Remove(where.Length - 2);
					where += ")";
					where = where.Replace("# ", " WHERE ").Replace("* ", " AND ").Replace("$ ", "OR ");

					cmd.CommandText = query + where;

					string[] modelPropertyNames;
					switch (request.DetailLevel)
					{
						case "all":
							modelPropertyNames = typeof(DataViewFindResponseAllModel).GetProperties().Select(property => property.Name).ToArray();
							break;
						case "admin":
							modelPropertyNames = typeof(DataViewFindResponseAdminModel).GetProperties().Select(property => property.Name).ToArray();
							break;
						case "basic":
						default:
							modelPropertyNames = typeof(DataViewFindResponseBasicModel).GetProperties().Select(property => property.Name).ToArray();
							break;
					}
					var bindPropertyNames = modelPropertyNames.Select(propertyName =>
					{
						return Regex.Replace(propertyName, @"([A-Z])", "_$1").Substring(1).ToLower();
					}).ToArray();
					var resultList = new List<object>();
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							object record;
							switch (request.DetailLevel)
							{
								case "all": record = new DataViewFindResponseAllModel(); break;
								case "admin": record = new DataViewFindResponseAdminModel(); break;
								case "basic":
								default: record = new DataViewFindResponseBasicModel(); break;
							}
							for (var index = 0; index < modelPropertyNames.Length; index++)
							{
								record.GetType().GetProperty(modelPropertyNames[index]).SetValue(record, reader[bindPropertyNames[index]] == DBNull.Value ? null : reader[bindPropertyNames[index]]);
							}
							resultList.Add(record);
						}
					}
					return resultList;
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public string GenerateQuery(DataViewDetailsModel dataView, string mainTable, List<string> requiredTableNames)
		{
			try
			{
				if (dataView.ViewSource.DataSourceFields.Count == 0)
				{
					return $"SELECT * from {mainTable}";
				}

				var query = "SELECT ";

				var apiDatabaseTableReferences = ApiDatabaseTableReferences.Instance;
				var mainTableIndex = apiDatabaseTableReferences.FindIndex((tableRef) => tableRef.TableName == mainTable);
				
				var isMainTablePrimaryKeyIncluded = false;
				dataView.ViewSource.DataSourceFields.ForEach((dataSourceField) =>
				{
					if (dataSourceField.CustomerAttribute.CustomerAttributeSource == mainTable
						&& dataSourceField.CustomerAttribute.CustomerAttributeName == apiDatabaseTableReferences[mainTableIndex].PrimaryKey)
					{
						isMainTablePrimaryKeyIncluded = true;
					}

					if (string.IsNullOrEmpty(dataSourceField.DataSourceFieldName)
						|| (string.IsNullOrEmpty(dataSourceField.CustomerAttribute.CustomerAttributeId))
						|| (string.IsNullOrEmpty(dataSourceField.CustomerAttribute.SystemAttributeId)))
					{
						return;
					}
					
					if (string.IsNullOrEmpty(dataSourceField.CustomerAttribute.SystemAttributeId))
					{
						query += dataSourceField.CustomerAttribute.CustomerAttributeSource
							+ "." + dataSourceField.CustomerAttribute.CustomerAttributeName
							+ " as " + dataSourceField.DataSourceFieldName
							+ ", ";
						if (requiredTableNames.IndexOf(dataSourceField.CustomerAttribute.CustomerAttributeSource) < 0)
						{
							requiredTableNames.Add(dataSourceField.CustomerAttribute.CustomerAttributeSource);
						}
					}
					else
					{
						query += dataSourceField.CustomerAttribute.SystemAttributeId + " as " + dataSourceField.DataSourceFieldName + ", ";

						var systemAttributeSource = dataSourceField.CustomerAttribute.SystemAttributeId.Split(".")[0];
						if (requiredTableNames.IndexOf(systemAttributeSource) < 0)
						{
							requiredTableNames.Add(systemAttributeSource);
						}
					}
				});
				if (!isMainTablePrimaryKeyIncluded)
				{
					query += $"{mainTable}.{apiDatabaseTableReferences[mainTableIndex].PrimaryKey} as {apiDatabaseTableReferences[mainTableIndex].PrimaryKey}, ";
					if (requiredTableNames.IndexOf(mainTable) < 0)
					{
						requiredTableNames.Add(mainTable);
					}
				}
				query = query.Remove(query.Length - 2);
				query += $" FROM {mainTable} ";

				// Implement Joins
				requiredTableNames.RemoveAt(requiredTableNames.IndexOf(mainTable));

				var requiredTableRelations = new List<ApiDatabaseTableRelationForJoin>();
				apiDatabaseTableReferences[mainTableIndex].ForeignKeys.ForEach((foreignKey) =>
				{
					if (requiredTableNames.Count == 0)
					{
						return;
					}
					requiredTableRelations.Add(new ApiDatabaseTableRelationForJoin
					{
						PrimaryTableName = apiDatabaseTableReferences[mainTableIndex].TableName,
						PrimaryTableForeignKey = foreignKey.ForeignKey,
						ForeignTableName = foreignKey.RelatedTableName,
						ForeignTablePrimaryKey = foreignKey.RelatedPrimaryKey
					});

					var foreignKeyTableNameIndex = requiredTableNames.IndexOf(foreignKey.RelatedTableName);
					if (foreignKeyTableNameIndex >= 0)
					{
						requiredTableNames.RemoveAt(requiredTableNames.IndexOf(foreignKey.RelatedTableName));
					}
				});

				var tryCount = 0;
				while (requiredTableNames.Count > 0)
				{
					tryCount ++;
					if (tryCount > 15)
					{
						break;
					}
					var additionalTableRelations = new List<ApiDatabaseTableRelationForJoin>();
					requiredTableRelations.ForEach((tableRelation) =>
					{
						var foreignTableName = tableRelation.ForeignTableName;
						var foreignTableIndex = requiredTableRelations.FindIndex((relation) => relation.PrimaryTableName == foreignTableName);
						if (foreignTableIndex >= 0)
						{
							return;
						}

						var referenceIndex = apiDatabaseTableReferences.FindIndex((reference) => reference.TableName == foreignTableName);
						if (referenceIndex < 0)
						{
							return;
						}
						apiDatabaseTableReferences[referenceIndex].ForeignKeys.ForEach((foreignKey) =>
						{
							if (requiredTableNames.Count == 0)
							{
								return;
							}

							additionalTableRelations.Add(new ApiDatabaseTableRelationForJoin
							{
								PrimaryTableName = apiDatabaseTableReferences[referenceIndex].TableName,
								PrimaryTableForeignKey = foreignKey.ForeignKey,
								ForeignTableName = foreignKey.RelatedTableName,
								ForeignTablePrimaryKey = foreignKey.RelatedPrimaryKey
							});

							var relatedTableNameIndex = requiredTableNames.IndexOf(foreignKey.RelatedTableName);
							if (relatedTableNameIndex >= 0)
							{
								requiredTableNames.RemoveAt(requiredTableNames.IndexOf(foreignKey.RelatedTableName));
							}
						});
					});
					requiredTableRelations.AddRange(additionalTableRelations);
				}

				requiredTableRelations.ForEach((relation) =>
				{
					query += $"LEFT JOIN {relation.ForeignTableName} ON {relation.PrimaryTableName}.{relation.PrimaryTableForeignKey} = {relation.ForeignTableName}.{relation.ForeignTablePrimaryKey} ";
				});

				return query;
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public Dictionary<string, object> GetDataView(DatabaseHelper dbHelper, DataViewGetRequestModel request)
		{
			try
			{
				Dictionary<string, object> result = null;
				using (var cmd = dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT "
						+ "data_views.company_id, data_views.create_datetime, data_views.create_user_id, "
						+ "data_views.data_source_id, data_views.data_filter_id, data_views.edit_datetime, "
						+ "data_views.edit_user_id, data_views.group_id, data_views.office_id, "
						+ "data_views.user_id, data_views.view_desc, data_views.view_field_list, data_views.view_field_settings, "
						+ "data_views.view_filter, data_views.view_id, data_views.view_name, data_views.view_query_generated, "
						+ "data_views.view_type, data_views.view_sort, data_views.view_status, "
						+ "customers.customer_name as company_name, company_offices.company_office_name as office_name, "
						+ "users.user_displayname "
						+ "FROM data_views "
						+ "LEFT JOIN customers "
						+ "ON data_views.company_id = customers.customer_id LEFT JOIN users "
						+ "ON data_views.user_id = users.user_id LEFT JOIN company_offices "
						+ "ON data_views.office_id = company_offices.company_office_id "
						+ "WHERE data_views.view_id = @view_id";
					cmd.Parameters.AddWithValue("@view_id", request.ViewId);

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							result = new Dictionary<string, object>()
							{
								{ "company_id", Convert.ToString(reader["company_id"]) },
								{ "create_datetime", Convert.ToString(reader["create_datetime"]) },
								{ "create_user_id", Convert.ToString(reader["create_user_id"]) },
								{ "data_source_id", Convert.ToString(reader["data_source_id"]) },
								{ "data_filter_id", Convert.ToString(reader["data_filter_id"]) },
								{ "edit_datetime", Convert.ToString(reader["edit_datetime"]) },
								{ "edit_user_id", Convert.ToString(reader["edit_user_id"]) },
								{ "group_id", Convert.ToString(reader["group_id"]) },
								{ "office_id", Convert.ToString(reader["office_id"]) },
								{ "user_id", Convert.ToString(reader["user_id"]) },
								{ "view_desc", Convert.ToString(reader["view_desc"]) },
								{ "view_field_list", Convert.ToString(reader["view_field_list"]) },
								{ "view_field_settings", Convert.ToString(reader["view_field_settings"]) },
								{ "view_filter", Convert.ToString(reader["view_filter"]) },
								{ "view_id", Convert.ToString(reader["view_id"]) },
								{ "view_name", Convert.ToString(reader["view_name"]) },
								{ "view_query_generated", Convert.ToString(reader["view_query_generated"]) },
								{ "view_type", Convert.ToString(reader["view_type"]) },
								{ "view_sort", Convert.ToString(reader["view_sort"]) },
								{ "view_status", Convert.ToString(reader["view_status"]) },
								{ "company_name", Convert.ToString(reader["company_name"]) },
								{ "office_name", Convert.ToString(reader["office_name"]) },
								{ "user_displayname", Convert.ToString(reader["user_displayname"]) }
							};
						}
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public DataViewDetailsModel GetDataViewDetails(DatabaseHelper dbHelper, string dataViewId)
		{
			try
			{
				using (var cmd = dbHelper.SpawnCommand())
				{
					cmd.CommandText = "SELECT "
						+ "data_views.view_id, data_views.view_name, "
						+ "data_sources.data_source_id, data_sources.data_source_name, "
						+ "data_source_fields.data_source_field_id, data_source_fields.data_source_field_name, "
						+ "data_view_filter.data_view_filter_name, data_view_filter.data_view_filter_sql, "
						+ "data_source_fields.customer_attribute_id, "
						+ "customer_attributes.customer_attribute_id, customer_attributes.customer_attribute_name, customer_attributes.customer_attribute_source, customer_attributes.system_attribute_id "
						+ "FROM data_views "
						+ "LEFT JOIN data_sources ON data_views.data_source_id = data_sources.data_source_id "
						+ "LEFT JOIN data_view_filter ON data_views.data_filter_id = data_view_filter.data_view_filter_id "
						+ "LEFT JOIN data_view_field_settings ON data_view_field_settings.data_view_id = data_views.view_id "
						+ "LEFT JOIN data_source_fields ON data_sources.data_source_id = data_source_fields.data_source_id AND data_view_field_settings.data_view_field_id = data_source_fields.data_source_field_id "
						+ "LEFT JOIN customer_attributes ON data_source_fields.customer_attribute_id = customer_attributes.customer_attribute_id "
						+ "WHERE data_views.view_id=@view_id";
					cmd.Parameters.AddWithValue("@view_id", dataViewId);
					
					DataViewDetailsModel result = null;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							if (result == null)
							{
								result = new DataViewDetailsModel
								{
									ViewId = ApiExtension.GetString(reader["view_id"]),
									ViewName = ApiExtension.GetString(reader["view_name"]),
									ViewFilter = new DataViewDetailsModel.DataFilterSimpleModel
									{
										DataViewFilterName = ApiExtension.GetString(reader["data_view_filter_name"]),
										DataViewFilterSql = ApiExtension.GetString(reader["data_view_filter_sql"])
									},
									ViewSource = new DataViewDetailsModel.DataSourceSimpleModel
									{
										DataSourceId = ApiExtension.GetString(reader["data_source_id"]),
										DataSourceName = ApiExtension.GetString(reader["data_source_name"]),
										DataSourceFields = new List<DataViewDetailsModel.DataSourceFieldSimpleModel>()
									}
								};
							}

							result.ViewSource.DataSourceFields.Add(new DataViewDetailsModel.DataSourceFieldSimpleModel
							{
								DataSourceFieldId = ApiExtension.GetString(reader["data_source_field_id"]),
								DataSourceFieldName = ApiExtension.GetString(reader["data_source_field_name"]),
								CustomerAttribute = new DataViewDetailsModel.CustomerAttributeSimpleModel
								{
									CustomerAttributeId = ApiExtension.GetString(reader["customer_attribute_id"]),
									CustomerAttributeName = ApiExtension.GetString(reader["customer_attribute_name"]),
									CustomerAttributeSource = ApiExtension.GetString(reader["customer_attribute_source"]),
									SystemAttributeId = ApiExtension.GetString(reader["system_attribute_id"])
								}
							});
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

		public int UpdateDataView(DatabaseHelper dbHelper, DataViewUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "data_views");
		}
	}
}
