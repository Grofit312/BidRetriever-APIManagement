using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.ProjectSourceManagement
{
	public class ProjectSource : BaseModel
	{
		public string user_id { get; set; }
		public string customer_id { get; set; }
		public string primary_project_id { get; set; }
		public string secondary_project_id { get; set; }
		public string project_source_status { get; set; }
	}
}
