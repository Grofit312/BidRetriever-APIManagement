using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.AnalyticDatasources;
using _440DocumentManagement.Services.Interface;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace _440DocumentManagement.Services.Concrete
{
	public class AnalyticDatasourcesManagementService : IAnalyticDatasourcesManagementService
	{
		private readonly IBaseService _baseService;
		private readonly IDbConnection _dbConnection;
		private readonly IAmazonLambda _lambdaClient;

		public AnalyticDatasourcesManagementService(
			IBaseService baseService,
			IDbConnection dbConnection,
			IAmazonLambda lambdaClient)
		{
			_baseService = baseService;
			_dbConnection = dbConnection;
			_lambdaClient = lambdaClient;
		}

		public string CreateRecord(AnalyticDatasourcesModel newRecord)
		{
			newRecord.CustomerId = newRecord.CustomerId ?? "default";
			var newRecordId = _baseService.CreateRecord(newRecord, Constants.ApiTables.ANALYTIC_DATASOURCES.TableName, Constants.ApiTables.ANALYTIC_DATASOURCES.PrimaryKey);
			return newRecordId;
		}

		public async Task<List<object>> ExecuteRecord(AnalyticDatasourcesExecuteRequestModel request)
		{
			try
			{
				AnalyticDatasourcesGetRequestModel recordGetRequest = new AnalyticDatasourcesGetRequestModel
				{
					AnalyticDatasourceId = request.AnalyticDatasourceId
				};

				var record = GetRecord(recordGetRequest);
				if (record == null)
				{
					return new List<object>();
				}

				if (!string.IsNullOrEmpty(record.AnalyticDatasourceSql))
				{
					using (var conn = _dbConnection)
					{
						conn.Open();
						using (var cmd = conn.CreateCommand())
						{
							cmd.CommandText = record.AnalyticDatasourceSql;

							cmd.AddWithValue("@customer_id", request.CustomerId);
							cmd.AddWithValue("@company_id", request.CompanyId);
							cmd.AddWithValue("@start_date", (object)request.AnalyticDatasourceStartdatetime ?? DBNull.Value);
							cmd.AddWithValue("@end_date", (object)request.AnalyticDatasourceEnddatetime ?? DBNull.Value);

							if (!string.IsNullOrEmpty(request.AdditionalFilters))
							{
								Dictionary<string, object> additionalFilters = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.AdditionalFilters);
								foreach (var filterKey in additionalFilters.Keys)
								{
									cmd.AddWithValue($"@{filterKey}", additionalFilters[filterKey]);
								}
							}

							var resultList = new List<object>();
							using (var reader = cmd.ExecuteReader())
							{
								var columns = new List<string>();
								for (var index = 0; index < reader.FieldCount; index ++)
								{
									columns.Add(reader.GetName(index));
								}

								while (reader.Read())
								{
									var row = new Dictionary<string, object>();
									var isValid = true;

									for (var index = 0; index < columns.Count; index ++)
									{
										if (reader[columns[index]] == DBNull.Value)
										{
											isValid = false;
											break;
										}

										row.Add(columns[index], reader[columns[index]]);
									}

									if (isValid)
									{
										resultList.Add(row);
									}
								}
							}

							return resultList;
						}
					}
				}
				else if (!string.IsNullOrEmpty(record.AnalyticDatasourceLambdaArn))
				{
					var payload = new Dictionary<string, object>();
					var response = await _lambdaClient.InvokeAsync(new InvokeRequest
					{
						FunctionName = record.AnalyticDatasourceLambdaArn,
						Payload = JsonConvert.SerializeObject(payload)
					});
					if (response.StatusCode == 200)
					{
						using (var streamReader = new StreamReader(response.Payload))
						{
							var stringified = streamReader.ReadToEnd();
							var values = JsonConvert.DeserializeObject<List<object>>(stringified);
							return values;
						}
					}
					else
					{
						return new List<object>();
					}
				}
				else
				{
					return new List<object>();
				}
			}
			catch (Exception ex)
			{
				throw new ApiException(ex.Message);
			}
		}

		public List<object> FindRecords(AnalyticDatasourcesFindRequestModel request)
		{
			var serviceRequest = new AnalyticDatasourcesFindRequestModelForService();
			serviceRequest.AnalyticDatasourceName = request.AnalyticDatasourceName;
			serviceRequest.AnalyticDatasourceType = request.AnalyticDatasourceType;
			if (string.IsNullOrEmpty(request.CustomerId))
			{
				serviceRequest.CustomerId = new string[]
				{
					"default"
				};
			}
			else
			{
				serviceRequest.CustomerId = new string[]
				{
					"default", request.CustomerId
				};
			}

			List<object> records =  _baseService.FindRecords<
				AnalyticDatasourcesFindRequestModelForService,
				AnalyticDatasourcesModel,
				AnalyticDatasourcesModel,
				AnalyticDatasourcesModel,
				AnalyticDatasourcesModel>(serviceRequest, Constants.ApiTables.ANALYTIC_DATASOURCES.TableName);
			return records;
		}

		public AnalyticDatasourcesModel GetRecord(AnalyticDatasourcesGetRequestModel request)
		{
			var record = _baseService.GetRecord<AnalyticDatasourcesGetRequestModel, AnalyticDatasourcesModel>(
				request,
				Constants.ApiTables.ANALYTIC_DATASOURCES.TableName);
			return record;
		}

		public int UpdateRecords(AnalyticDatasourcesUpdateRequestModel request)
		{
			int updatedCount = _baseService.UpdateRecords(request, Constants.ApiTables.ANALYTIC_DATASOURCES.TableName);
			return updatedCount;
		}
	}
}
