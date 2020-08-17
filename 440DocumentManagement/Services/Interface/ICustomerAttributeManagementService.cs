using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.CustomerAttribute;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface ICustomerAttributeManagementService
	{
		string CreateCustomerAttribute(DatabaseHelper dbHelper, CustomerAttributeModel newRecord);
		List<object> FindCustomerAttributes(DatabaseHelper dbHelper, CustomerAttributeFindRequestModel request);
		int UpdateCustomerAttribute(DatabaseHelper dbHelper, CustomerAttributeUpdateRequestModel request);
	}
}
