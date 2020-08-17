using _440DocumentManagement.Models.Dashboard;
using _440DocumentManagement.Services.Interface;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class DashboardManagementService : IDashboardManagementService
	{
		private readonly IBaseService _baseService;

		public DashboardManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
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
