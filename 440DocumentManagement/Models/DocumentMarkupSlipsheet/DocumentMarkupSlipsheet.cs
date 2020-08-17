using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.DocumentMarkupSlipsheet
{
    public class DocumentMarkupSlipsheet : BaseModel
    {
        public string markup_id { get; set; }
        public string doc_id { get; set; }
        public bool active_annotations_only { get; set; }
        public bool copy_transactions_only { get; set; }
    }
}
