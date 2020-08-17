using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.DocMarkupAnnotations
{
	public class DocMarkupAnnotationTransaction
	{
		public string annotation_transaction_id { get; set; }
		public string annotation_id { get; set; }
		public DateTime? create_datetime { get; set; }
		public string create_userid { get; set; }
		public string annotation_transaction_type { get; set; }
		public string annotation_transaction_data { get; set; }
		public string annotation_transaction_status { get; set; }
	}
}
