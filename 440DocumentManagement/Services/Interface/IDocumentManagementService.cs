using _440DocumentManagement.Helpers;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	public interface IDocumentManagementService
	{
		void CreateFolderTransactionLog(DatabaseHelper dbHelper, Dictionary<string, object> data, string operationType);
		string GenerateDocRevision(DatabaseHelper dbHelper, string customerId, Dictionary<string, object> currentDoc, List<Dictionary<string, object>> documents);
		string GeneratePlanFileName(DatabaseHelper dbHelper, string projectId, string docName, string docNumber, string docRevision);
		Dictionary<string, string> GetDocumentComparison(DatabaseHelper dbHelper, string docId, string docRevision);
		List<Dictionary<string, string>> GetDocumentComparisons(DatabaseHelper dbHelper, List<Dictionary<string, string>> docRevisions);
		Dictionary<string, string> GetDocumentRevision(DatabaseHelper dbHelper, string docId);
		List<Dictionary<string, string>> GetDocumentRevisions(DatabaseHelper dbHelper, string docId);
		string GetNextRevisionDocId(DatabaseHelper dbHelper, string currentDocId);
		string GetPreviousRevisionDocId(DatabaseHelper dbHelper, string currentDocId);
		void RemoveComparison(DatabaseHelper dbHelper, string docId);
		Dictionary<string, string> RetrieveDocument(DatabaseHelper dbHelper, string docId);
		List<Dictionary<string, string>> RetrieveMatchedDocumentsWithKeyAttributes(DatabaseHelper dbHelper, string docId, string projectId, string docNumber, string docPageNumber, string docSubProject);
        List<Dictionary<string, object>> FindFolderTransactionLogs(DatabaseHelper dbHelper, string docId);
        Dictionary<string, string> GetInfoForKeyAttributeUpdate(DatabaseHelper dbHelper, string docId);
    }
}
