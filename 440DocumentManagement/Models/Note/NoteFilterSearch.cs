namespace _440DocumentManagement.Models
{
    public class NoteFilterSearch : BaseModel
    {
        public string annotation_id { get; set; }
        public string company_id { get; set; }
        public string detail_level { get; set; }
        public string doc_id { get; set; }
        public string event_id { get; set; }
        public string folder_id { get; set; }
        public string markup_id { get; set; }
        public string note_id { get; set; }
        public string note_type { get; set; }
        public string office_id { get; set; }
        public string project_id { get; set; }
        public string user_id { get; set; }
        public bool return_child_notes { get; set; } = false;
    }
}
