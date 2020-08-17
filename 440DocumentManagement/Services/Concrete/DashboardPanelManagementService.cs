using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DashboardPanel;
using _440DocumentManagement.Services.Interface;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class DashboardPanelManagementService : IDashboardPanelManagementService
	{
		private readonly IBaseService _baseService;

		public DashboardPanelManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateRecord(DashboardPanelModel newRecord)
		{
			var newRecordId = _baseService.CreateRecord(
				newRecord,
				Constants.ApiTables.DASHBOARD_PANEL.TableName,
				Constants.ApiTables.DASHBOARD_PANEL.PrimaryKey);
			return newRecordId;
		}

		public List<object> FindRecords(DashboardPanelFindRequestModel request)
		{
			string where = "";
			if (string.IsNullOrEmpty(request.DashboardId))
			{
				request.CustomerId = request.CustomerId ?? "default";
				request.DeviceId = request.DeviceId ?? "default";
			}
			if (!string.IsNullOrEmpty(request.CustomerId))
			{
				where += $"\"{Constants.ApiTables.DASHBOARD.TableName}\".customer_id = '{request.CustomerId}' * ";
				request.CustomerId = null;
			}
			if (!string.IsNullOrEmpty(request.DeviceId))
			{
				where += $"\"{Constants.ApiTables.DASHBOARD.TableName}\".device_id = '{request.DeviceId}' * ";
				request.DeviceId = null;
			}
			if (!string.IsNullOrEmpty(request.UserId))
			{
				where += $"\"{Constants.ApiTables.DASHBOARD.TableName}\".user_id = '{request.UserId}' * ";
				request.UserId = null;
			}
			if (!string.IsNullOrEmpty(request.OfficeId))
			{
				where += $"\"{Constants.ApiTables.DASHBOARD.TableName}\".office_id = '{request.OfficeId}' * ";
				request.OfficeId = null;
			}
			if (!string.IsNullOrEmpty(where))
			{
				where = where.Remove(where.Length - 3);
			}
			where = where.Replace("* ", "AND ");

			var records = _baseService.FindRecords<
				DashboardPanelFindRequestModel,
				DashboardPanelModel,
				DashboardPanelModel,
				DashboardPanelModel,
				DashboardPanelModel>(
				request,
				Constants.ApiTables.DASHBOARD_PANEL.TableName,
				"",
				$"LEFT JOIN \"{Constants.ApiTables.DASHBOARD.TableName}\" ON \"{Constants.ApiTables.DASHBOARD.TableName}\".{Constants.ApiTables.DASHBOARD.PrimaryKey} = \"{Constants.ApiTables.DASHBOARD_PANEL.TableName}\".dashboard_id",
				where
			);
			return records;
		}

		public DashboardPanelModel GetRecord(DashboardPanelGetRequestModel request)
		{
			var record = _baseService.GetRecord<DashboardPanelGetRequestModel, DashboardPanelModel>(
				request,
				Constants.ApiTables.DASHBOARD_PANEL.TableName);
			return record;
		}

		public int UpdateRecords(DashboardPanelUpdateRequestModel request)
		{
			var updatedCount = _baseService.UpdateRecords(request, Constants.ApiTables.DASHBOARD_PANEL.TableName);
			return updatedCount;
		}
	}
}
