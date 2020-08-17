using _440DocumentManagement.Models;
using _440DocumentManagement.Models.DocMarkupAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Helpers
{
    public class FindDocumentCriteria
    {
        public string doc_id { get; set; }
        public string user_id { get; set; }
    }

    public class FindDocumentLog
    {
        public string doc_id { get; set; }
        public string user_id { get; set; }
    }


    public class DataViews
    {
        public string company_id { get; set; }
        public string group_id { get; set; }
        public string office_id { get; set; }
        public string user_id { get; set; }
        public string view_id { get; set; }
        public string view_name { get; set; }
        public string view_desc { get; set; }
        public DateTime? create_datetime { get; set; }
        public DateTime? edit_datetime { get; set; }
        public string customer_name { get; set; }
        public string display_name { get; set; }
        public string company_office_name { get; set; }

        public string view_type { get; set; }
    }

    public class DataViewsSearchCreteria : BaseModel
    {
        public string company_id { get; set; }
        public string group_id { get; set; }
        public string office_id { get; set; }
        public string user_id { get; set; }
        public string view_id { get; set; }
        public string view_type { get; set; }
    }
    public class DataViewFilterSearchCreteria : BaseModel
    {
        public string customer_id { get; set; }
        public string data_source_id { get; set; }
        public string user_id { get; set; }
    }

    public class FindCompaniesCreteria : BaseModel
    {
        public string company_name { get; set; }
        public string company_domain { get; set; }
        public string company_type { get; set; }
        public string company_service_area { get; set; }
        public string company_record_source { get; set; }
        public string company_state { get; set; }
        public string company_zip { get; set; }
        public string company_status { get; set; }
        public string detail_level { get; set; }
        public string customer_id { get; set; }
    }
    public class FindDeviceLogCreteria : BaseModel
    {
        public string doc_id { get; set; }
        public string file_id { get; set; }
        public string function_name { get; set; }
        public string operation_name { get; set; }
        public string operation_status { get; set; }
        public string project_id { get; set; }
        public string submission_id { get; set; }
        public string device_id { get; set; }
    }
}
