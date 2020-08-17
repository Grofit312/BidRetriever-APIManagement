using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSourceField;
using _440DocumentManagement.Services.Interface;

namespace _440DocumentManagement.Services.Concrete
{
	public class DataSourceFieldManagementService : IDataSourceFieldManagementService
	{
		private readonly IBaseService _baseService;

		public DataSourceFieldManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateDataSourceField(DatabaseHelper dbHelper, DataSourceFieldModel newRecord)
		{
			newRecord.DataSourceFieldStatus = newRecord.DataSourceFieldStatus ?? "active";
			newRecord.RequiredField = newRecord.RequiredField ?? false;
			newRecord.DataSourceFieldDisplayname = newRecord.DataSourceFieldDisplayname ?? newRecord.DataSourceFieldName;

			return _baseService.CreateRecord(newRecord, "data_source_fields", "data_source_field_id");
		}

		public DataSourceFieldModel GetDataSourceField(DatabaseHelper dbHelper, DataSourceFieldGetRequestModel request)
		{
			return _baseService.GetRecord<DataSourceFieldGetRequestModel, DataSourceFieldModel>(request, "data_source_fields");
		}

		public int UpdateDataSourceField(DatabaseHelper dbHelper, DataSourceFieldUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "data_source_fields");
		}
	}
}
