using _440DocumentManagement.Models.Dashboard;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDashboardManagementService
	{
		string CreateRecord(DashboardModel newRecord);
		List<object> FindRecords(DashboardFindRequestModel request);
		List<object> GetAnalyticData(GetAnalyticDataRequestModel request);
		DashboardModel GetRecord(DashboardGetRequestModel request);
		int UpdateRecords(DashboardUpdateRequestModel request);
	}
}
