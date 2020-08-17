using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSourceField;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDataSourceFieldManagementService
	{
		string CreateDataSourceField(DatabaseHelper dbHelper, DataSourceFieldModel newRecord);
		DataSourceFieldModel GetDataSourceField(DatabaseHelper dbHelper, DataSourceFieldGetRequestModel request);
		int UpdateDataSourceField(DatabaseHelper dbHelper, DataSourceFieldUpdateRequestModel request);
	}
}
