using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataSource;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDataSourceManagementService
	{
		string CreateDataSource(DatabaseHelper dbHelper, DataSourceModel dataSource);
		List<Dictionary<string, object>> FindDataSources(DatabaseHelper dbHelper, DataSourceFindRequestModel request);
		Dictionary<string, object> GetDataSource(DatabaseHelper dbHelper, DataSourceGetRequestModel request);
		int UpdateDataSource(DatabaseHelper dbHelper, DataSourceUpdateRequestModel request);
	}
}
