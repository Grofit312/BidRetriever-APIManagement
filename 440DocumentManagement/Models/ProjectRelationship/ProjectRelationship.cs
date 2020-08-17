using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.ProjectRelationship
{
    public class ProjectRelationship : BaseModel
    {
        public string project_relationship_id { get; set; }
        public string company_id { get; set; }
        public string contact_id { get; set; }
        public string project_id { get; set; }
        public string project_relationship_display_name { get; set; }
        public string project_relationship_status { get; set; }
        public string project_relationship_type_id { get; set; }
    }

    public class ProjectRelationshipCreteria
    {
        public string project_id { get; set; }
        public string company_id { get; set; }
        public string contact_id { get; set; }
        public string detail_level { get; set; }
    }
}
