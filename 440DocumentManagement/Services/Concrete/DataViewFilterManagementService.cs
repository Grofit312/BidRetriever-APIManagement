using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataViewFilterModel;
using _440DocumentManagement.Services.Interface;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class DataViewFilterManagementService : IDataViewFilterManagementService
	{
		private readonly IBaseService _baseService;

		public DataViewFilterManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateDataViewFilter(DatabaseHelper dbHelper, DataViewFilterModel newRecord)
		{
			if (string.IsNullOrEmpty(newRecord.DataViewFilterStatus))
			{
				newRecord.DataViewFilterStatus = "active";
			}
			return _baseService.CreateRecord(newRecord, "data_view_filter", "data_view_filter_id");
		}

		public List<object> FindDataViewFilters(DatabaseHelper dbHelper, DataViewFilterFindRequestModel request)
		{
			var extendedRequest = new DataViewFilterFindRequestExtendedModel()
			{
				CustomerId = new string[] { "default" },
				DataSourceId = request.DataSourceId,
				UserId = request.UserId,
				DataViewFilterStatus = request.DataViewFilterStatus ?? "active"
			};
			if (!string.IsNullOrEmpty(request.CustomerId))
			{
				extendedRequest.CustomerId = new string[] { "default", request.CustomerId };
			}

			return _baseService.FindRecords<
				DataViewFilterFindRequestExtendedModel,
				DataViewFilterModel,
				DataViewFilterModel,
				DataViewFilterModel,
				DataViewFilterModel>(extendedRequest, "data_view_filter");
		}

		public DataViewFilterModel GetDataViewFilter(DatabaseHelper dbHelper, DataViewFilterGetRequestModel request)
		{
			return _baseService.GetRecord<DataViewFilterGetRequestModel, DataViewFilterModel>(request, "data_view_filter");
		}

		public int UpdateDataViewFilter(DatabaseHelper dbHelper, DataViewFilterUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "data_view_filter");
		}
	}
}
