using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models.CustomerAttribute;
using _440DocumentManagement.Services.Interface;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Concrete
{
	public class CustomerAttributeManagementService : ICustomerAttributeManagementService
	{
		private readonly IBaseService _baseService;

		public CustomerAttributeManagementService(
			IBaseService baseService)
		{
			_baseService = baseService;
		}

		public string CreateCustomerAttribute(DatabaseHelper dbHelper, CustomerAttributeModel newRecord)
		{
			newRecord.CustomerId = newRecord.CustomerId ?? "default";
			newRecord.CustomerAttributeStatus = newRecord.CustomerAttributeStatus ?? "active";
			newRecord.CustomerAttributeDisplayname = newRecord.CustomerAttributeDisplayname ?? newRecord.CustomerAttributeName;

			return _baseService.CreateRecord(newRecord, "customer_attributes", "customer_attribute_id");
		}

		public List<object> FindCustomerAttributes(DatabaseHelper dbHelp, CustomerAttributeFindRequestModel request)
		{
			var extendedRequest = new CustomerAttributeFindRequestExtendedModel()
			{
				CustomerId = new string[] { "default" },
				CustomerAttributeStatus = request.CustomerAttributeStatus ?? "active"
			};
			if (!string.IsNullOrEmpty(request.CustomerId))
			{
				extendedRequest.CustomerId = new string[] { "default", request.CustomerId };
			}
			return _baseService.FindRecords<
				CustomerAttributeFindRequestExtendedModel,
				CustomerAttributeModel,
				CustomerAttributeModel,
				CustomerAttributeModel,
				CustomerAttributeModel
			>(extendedRequest, "customer_attributes");
		}

		public int UpdateCustomerAttribute(DatabaseHelper dbHelper, CustomerAttributeUpdateRequestModel request)
		{
			return _baseService.UpdateRecords(request, "customer_attributes");
		}
	}
}
