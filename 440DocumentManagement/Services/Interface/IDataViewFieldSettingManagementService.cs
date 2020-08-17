using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDataViewFieldSettingManagementService
	{
		string CreateDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingModel newRecord);
		List<Dictionary<string, object>> FindDataViewFieldSettings(DatabaseHelper dbHelper, DataViewFieldSettingFindRequestModel request);
		DataViewFieldSettingModel GetDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingGetRequestModel request);
		int UpdateDataViewFieldSetting(DatabaseHelper dbHelper, DataViewFieldSettingUpdateRequestModel request);
	}
}
