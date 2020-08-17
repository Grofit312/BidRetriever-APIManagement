using _440DocumentManagement.Helpers;
using System;
using System.Collections.Generic;

namespace _440DocumentManagement.Services.Interface
{
	// Q - Request Class
	// S - Response Class
	// SBA - Response (detail_level=basic) Class
	// SAL - Response (detail_level = all) Class
	// SAD - Response (detail_level=admin) Class
	public interface IBaseService
	{
		string CreateRecord<Q>(Q newRecord, string tableName, string primaryKey);
		List<object> FindRecords<Q, SBA, SAL, SAD, SCOM>(Q request, string tableName, string additionalResponseParams = "", string joinQueries = "", string whereQuries = "")
			where SBA: new()
			where SAL: new()
			where SAD: new()
			where SCOM: new();
		S GetRecord<Q, S>(Q request, string tableName) where S: new();
		int RemoveRecords<Q>(Q request, string tableName);
		int UpdateRecords<Q>(Q request, string tableName);
	}
}
