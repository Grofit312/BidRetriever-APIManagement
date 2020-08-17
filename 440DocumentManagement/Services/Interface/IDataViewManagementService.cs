using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.DataView;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDataViewManagementService
	{
		string CreateDataView(DatabaseHelper dbHelper, DataViewModel newRecord);
		List<object> FindDataViews(DatabaseHelper dbHelper, DataViewFindRequestModel search);
		string GenerateQuery(DataViewDetailsModel dataView, string mainTable, List<string> requiredTableNames);
		Dictionary<string, object> GetDataView(DatabaseHelper dbHelper, DataViewGetRequestModel request);
		DataViewDetailsModel GetDataViewDetails(DatabaseHelper dbHelper, string dataViewId);
		int UpdateDataView(DatabaseHelper dbHelper, DataViewUpdateRequestModel request);
	}
}
