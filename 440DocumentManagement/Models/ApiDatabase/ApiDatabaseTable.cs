using System.Collections.Generic;

namespace _440DocumentManagement.Models.ApiDatabase
{
	public class ApiDatabaseTableRelationForJoin
	{
		public string PrimaryTableName { get; set; }
		public string PrimaryTableForeignKey { get; set; }
		public string ForeignTableName { get; set; }
		public string ForeignTablePrimaryKey { get; set; }
	}

	public class ApiDatabaseTableRelation
	{
		public string ForeignKey { get; set; }
		public string RelatedTableName { get; set; }
		public string RelatedPrimaryKey { get; set; }
	}

	public class ApiDatabaseTable
	{
		public string TableName { get; set; }
		public string PrimaryKey { get; set; }
		public List<ApiDatabaseTableRelation> ForeignKeys { get; set; }
	}

	public class ApiDatabaseTableDetails
	{
		public string TableName { get; set; }
		public string PrimaryKey { get; set; }
	}
}
