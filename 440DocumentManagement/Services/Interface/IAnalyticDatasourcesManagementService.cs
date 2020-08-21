using _440DocumentManagement.Models.AnalyticDatasources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _440DocumentManagement.Services.Interface
{
	public interface IAnalyticDatasourcesManagementService
	{
		string CreateRecord(AnalyticDatasourcesModel newRecord);
		Task<List<object>> ExecuteRecord(AnalyticDatasourcesExecuteRequestModel request);
		List<object> FindRecords(AnalyticDatasourcesFindRequestModel request);
		AnalyticDatasourcesModel GetRecord(AnalyticDatasourcesGetRequestModel request);
		int UpdateRecords(AnalyticDatasourcesUpdateRequestModel request);
	}
}
