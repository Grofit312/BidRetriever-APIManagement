using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.SystemAttribute;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface ISystemAttributeManagementService
	{
		string CreateSystemAttribute(DatabaseHelper dbHelper, SystemAttributeModel newRecord);
		List<object> FindSystemAttributes(DatabaseHelper dbHelper, SystemAttributeFindRequestModel request);
		void InitializeSystemAttributes(DatabaseHelper dbHelper);
		int UpdateSystemAttribute(DatabaseHelper dbHelper, SystemAttributeUpdateRequestModel request);
	}
}
