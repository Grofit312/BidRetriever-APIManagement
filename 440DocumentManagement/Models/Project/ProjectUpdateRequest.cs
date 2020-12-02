namespace _440DocumentManagement.Models
{
	public class ProjectUpdateRequest
	{
		// Search Parameter
		public string search_project_id { get; set; }

		// Update Parameters
		public string project_name { get; set; }
		public string project_displayname { get; set; }
		public string project_address1 { get; set; }
		public string project_address2 { get; set; }
		public string project_city { get; set; }
		public string project_state { get; set; }
		public string project_zip { get; set; }
		public string project_country { get; set; }
		public string project_service_area { get; set; }
		public string project_number { get; set; }
		public string project_owner_name { get; set; }
		public string project_desc { get; set; }
		public string project_bid_datetime { get; set; }
		public string project_type { get; set; }
		public string status { get; set; }
		public string auto_update_status { get; set; }
		public string customer_source_sys_id { get; set; }
		public string project_password { get; set; }
		public string project_timezone { get; set; }
		public string source_url { get; set; }
		public string source_username { get; set; }
		public string source_password { get; set; }
		public string source_token { get; set; }
		public string source_sys_type_id { get; set; }
		public string project_notes { get; set; }
		public string project_process_status { get; set; }
		public string project_process_message { get; set; }
		public int project_rating { get; set; }
		public string project_contract_type { get; set; }
		public string project_stage { get; set; }
		public string project_segment { get; set; }
		public string project_building_type { get; set; }
		public string project_labor_requirement { get; set; }
		public int project_value { get; set; }
		public string project_size { get; set; }
		public string project_construction_type { get; set; }
		public string project_award_status { get; set; }
		public string source_company_id { get; set; }
		public string source_user_id { get; set; }
		public string project_assigned_office_id { get; set; }
		public string project_assigned_office_name { get; set; }

		// Update Admin Parameters
		public string project_parent_id { get; set; }
		public string project_admin_user_id { get; set; }
		public string project_customer_id { get; set; }

		public string source_project_id { get; set; }
		//public string contact_firstname { get; set; }
		//public string contact_lastname { get; set; }
		public string source_company_contact_id { get; set; }

        public int num_proj_sources { get; set; }

		public string source_company_displayname { get; set; }
		public string source_contact_displayname { get; set; }
		public string source_contact_email { get; set; }
		public string source_contact_phone { get; set; }
	}
}
