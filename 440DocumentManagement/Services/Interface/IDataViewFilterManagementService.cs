using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataViewFilterModel;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDataViewFilterManagementService
	{
		string CreateDataViewFilter(DatabaseHelper dbHelper, DataViewFilterModel newRecord);
		List<object> FindDataViewFilters(DatabaseHelper dbHelper, DataViewFilterFindRequestModel request);
		DataViewFilterModel GetDataViewFilter(DatabaseHelper dbHelper, DataViewFilterGetRequestModel request);
		int UpdateDataViewFilter(DatabaseHelper dbHelper, DataViewFilterUpdateRequestModel request);
	}
}
