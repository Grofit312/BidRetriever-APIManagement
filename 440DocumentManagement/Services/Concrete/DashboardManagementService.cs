using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.Dashboard;
using _440DocumentManagement.Services.Interface;
using System;
using System.Collections.Generic;
using System.Data;

namespace _440DocumentManagement.Services.Concrete
{
	public class DashboardManagementService : IDashboardManagementService
	{
		private readonly IBaseService _baseService;
		private readonly IDbConnection _dbConnection;

		public DashboardManagementService(
			IBaseService baseService,
			IDbConnection dbConnection)
		{
			_baseService = baseService;
			_dbConnection = dbConnection;
		}

		public string CreateRecord(DashboardModel newRecord)
		{
			newRecord.CustomerId = newRecord.CustomerId ?? "default";
			newRecord.DeviceId = newRecord.DeviceId ?? "default";

			var newRecordId = _baseService.CreateRecord(
				newRecord,
				Constants.ApiTables.DASHBOARD.TableName,
				Constants.ApiTables.DASHBOARD.PrimaryKey);
			return newRecordId;
		}

		public List<object> FindRecords(DashboardFindRequestModel request)
		{
			request.CustomerId = request.CustomerId ?? "default";
			request.DeviceId = request.DeviceId ?? "default";

			var records = _baseService.FindRecords<
				DashboardFindRequestModel,
				DashboardModel,
				DashboardModel,
				DashboardModel,
				DashboardModel>(request, Constants.ApiTables.DASHBOARD.TableName);
			return records;
		}

		public List<object> GetAnalyticData(GetAnalyticDataRequestModel request)
		{
			try
			{
				using (var conn = _dbConnection)
				{
					conn.Open();
					using (var cmd = conn.CreateCommand())
					{
						var query = "SELECT ";
						switch (request.AnalyticType)
						{
							case "project_stage":
								query += "projects.project_stage, ";
								query += "COUNT(projects.project_stage) AS total_stage ";
								break;
							case "bid_month":
								query += "projects.bid_month, ";
								query += "COUNT(projects.bid_month) AS total_invites ";
								break;
						}
						query += "FROM projects ";
						query += "LEFT JOIN customer_companies ON projects.source_company_id = customer_companies.company_id ";
						query += "RIGHT JOIN customers ON projects.project_customer_id = customers.customer_id ";
						query += "LEFT JOIN users ON projects.project_admin_user_id = users.user_id ";
						query += "WHERE ";
						query += "projects.project_customer_id = @customer_id ";
						cmd.AddWithValue("@customer_id", request.CustomerId);
						if (!string.IsNullOrEmpty(request.CompanyId))
						{
							query += "AND projects.source_company_id = @source_company_id ";
							cmd.AddWithValue("@source_company_id", request.CompanyId);
						}
						query += "GROUP BY ";
						switch (request.AnalyticType)
						{
							case "project_stage":
								query += "projects.project_stage";
								break;
							case "bid_month":
								query += "projects.bid_month";
								break;
						}

						cmd.CommandText = query;
						var result = new List<object>();
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								switch (request.AnalyticType)
								{
									case "project_stage":
										result.Add(new Dictionary<string, object>
										{
											{ "project_stage", reader["project_stage"] },
											{ "total_stage", reader["total_stage"] }
										});
										break;
									case "bid_month":
										result.Add(new Dictionary<string, object>
										{
											{ "bid_month", reader["bid_month"] },
											{ "total_invites", reader["total_invites"] }
										});
										break;
								}
							}
						}

						return result;
					}
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public DashboardModel GetRecord(DashboardGetRequestModel request)
		{
			var record = _baseService.GetRecord<DashboardGetRequestModel, DashboardModel>(
				request,
				Constants.ApiTables.DASHBOARD.TableName);
			return record;
		}

		public int UpdateRecords(DashboardUpdateRequestModel request)
		{
			var updatedCount = _baseService.UpdateRecords(request, Constants.ApiTables.DASHBOARD.TableName);
			return updatedCount;
		}
	}
}
