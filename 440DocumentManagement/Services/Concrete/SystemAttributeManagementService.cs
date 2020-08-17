using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.SystemAttribute;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _440DocumentManagement.Services.Concrete
{
	public class SystemAttributeManagementService : ISystemAttributeManagementService
	{
		private readonly IBaseService _baseService;

		public SystemAttributeManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateSystemAttribute(DatabaseHelper dbHelper, SystemAttributeModel newRecord)
		{
			if (string.IsNullOrEmpty(newRecord.SystemAttributeStatus))
			{
				newRecord.SystemAttributeStatus = "active";
			}
			return _baseService.CreateRecord(newRecord, "system_attributes", "system_attribute_id");
		}

		public List<object> FindSystemAttributes(DatabaseHelper dbHelper, SystemAttributeFindRequestModel request)
		{
			request.SystemAttributeStatus = request.SystemAttributeStatus ?? "active";
			return _baseService.FindRecords<
				SystemAttributeFindRequestModel,
				SystemAttributeModel,
				SystemAttributeModel,
				SystemAttributeModel,
				SystemAttributeModel
			>(request, "system_attributes");
		}

		public void InitializeSystemAttributes(DatabaseHelper dbHelper)
		{
			try
			{
				using (var cmd = dbHelper.SpawnCommand())
				{
					// Remove all content on system_attributes table
					cmd.CommandText = "TRUNCATE TABLE public.system_attributes";
					cmd.ExecuteNonQuery();

					// Get All Table Names
					var tableNames = new List<string>();
					cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema='public'";
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							tableNames.Add(Convert.ToString(reader["table_name"]));
						}
					}

					var newRecords = new List<SystemAttributeModel>();
					for (var index = 0; index < tableNames.Count; index ++)
					{
						cmd.CommandText = "SELECT c.column_name as column_name, c.udt_name as column_type, pgd.description as column_description "
							+ "FROM pg_catalog.pg_statio_all_tables AS st "
							+ "INNER JOIN pg_catalog.pg_description pgd ON (pgd.objoid = st.relid) "
							+ "RIGHT OUTER JOIN information_schema.columns c ON (pgd.objsubid = c.ordinal_position AND c.table_schema = st.schemaname AND c.table_name = st.relname) "
							+ $"WHERE table_schema = 'public' AND table_name = '{tableNames[index]}'";
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var columnDescription = Convert.ToString(reader["column_description"]);
								var columnName = Convert.ToString(reader["column_name"]);
								var columnType = Convert.ToString(reader["column_type"]);
								var defaultHeading = string.Join(" ", columnName.Split("_").Select(item => item.First().ToString().ToUpper() + item.Substring(1)));
								var defaultAlignment = "left";
								if (columnName.Contains("bool"))
								{
									defaultAlignment = "center";
								}
								else if (columnName.Contains("int") || columnName.Contains("float") || columnName.Contains("decimal"))
								{
									defaultAlignment = "right";
								}

								newRecords.Add(new SystemAttributeModel
								{
									SystemAttributeId = $"{tableNames[index]}.{columnName}",
									SystemAttributeName = columnName,
									SystemAttributeDatatype = columnType,
									SystemAttributeSource = tableNames[index],
									SystemAttributeStatus = "active",
									SystemAttributeDesc = columnDescription,
									CreateUserId = null,
									EditUserId = null,
									DefaultAlignment = defaultAlignment,
									DefaultWidth = 15,
									DefaultFormat = null,
									DefaultHeading = defaultHeading
								});
							}
						}
					}

					for (var index = 0; index < newRecords.Count; index ++)
					{
						CreateSystemAttribute(dbHelper, newRecords[index]);
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public int UpdateSystemAttribute(DatabaseHelper dbHelper, SystemAttributeUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "system_attributes");
		}
	}
}
