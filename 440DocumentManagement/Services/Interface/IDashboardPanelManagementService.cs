using _440DocumentManagement.Models.DashboardPanel;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDashboardPanelManagementService
	{
		string CreateRecord(DashboardPanelModel newRecord);
		List<object> FindRecords(DashboardPanelFindRequestModel request);
		DashboardPanelModel GetRecord(DashboardPanelGetRequestModel request);
		int UpdateRecords(DashboardPanelUpdateRequestModel request);
	}
}
