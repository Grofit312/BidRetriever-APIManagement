using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.PermissionManagement
{
    public class PermissionsFilter : BaseModel
{
        public string object_displayname { get; set; }
        public string permission_id { get; set; }
        public string permission_level { get; set; }
        public string permission_status { get; set; }
        public string user_displayname { get; set; }
    }

    public class PermissionFilterSearch : BaseModel
    {
        public string company_id { get; set; }
        public string object_id { get; set; }
        public string object_type { get; set; }
        public string user_id { get; set; }
        public string detail_level { get; set; }
    }
}
