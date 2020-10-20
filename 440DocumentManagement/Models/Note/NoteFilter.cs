using System.Collections.Generic;

namespace _440DocumentManagement.Models
{
  public class NoteFilter : BaseModel
  {
    public string note_company_id { get; set; }
    public string note_desc { get; set; }
    public string note_display_name { get; set; }
    public string note_id { get; set; }
    public string note_type { get; set; }
    public string note_parent_id { get; set; }
    public string created_user_id { get; set; }
    public string note_parent_type { get; set; }
    public string note_priority { get; set; }
    public int? note_relevance_number { get; set; }
    public string note_status { get; set; }
    public string note_subject { get; set; }
    public string note_timeline_displayname { get; set; }
    public int? note_vote_count { get; set; }

  }
}
