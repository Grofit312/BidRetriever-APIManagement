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


  public class CustomModel
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Subject { get; set; }
    public string ParentId { get; set; }
    public string UserId { get; set; }
    public string NoteType { get; set; }
    public string CompanyId { get; set; }
    public string NoteParentType { get; set; }
    public string CreatedDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserRole { get; set; }

    public List<CustomModel> Children { get; set; }
    public CustomModel()
    {
      Children = new List<CustomModel>();
    }


    public class FileUploadModel
    {
      public byte[] FileBytes { get; set; }
      public string FileName { get; set; }
    }
  }
}
