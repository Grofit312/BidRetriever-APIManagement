using _440DocumentManagement.Models.ApiDatabase;
using System.ComponentModel;

public static class Constants
{
	public const string BID_RETRIEVER_EMAIL_ADDRESS = "donotreply@bidretriever.net";
	public const int TOKEN_LIFETIME = 4;
	public const string JWT_SECRET_KEY = "bidretrieverdotnet123456";
	public const string TABLE_PROJECT_STANDARDIZATION = "940ProjectStandardization";

	public static class ApiStatus
	{
		public static string ERROR = "error";
		public static string SUCCESS = "success";
	}

	public static class ApiTables
	{
		public static readonly ApiDatabaseTableDetails DASHBOARD = new ApiDatabaseTableDetails
		{
			TableName = "dashboards",
			PrimaryKey = "dashboard_id"
		};
		public static readonly ApiDatabaseTableDetails DASHBOARD_PANEL = new ApiDatabaseTableDetails
		{
			TableName = "dashboard_panel",
			PrimaryKey = "panel_id"
		};
	}
}
