namespace _440DocumentManagement.Models
{
    public class UnpublishedFindRequest
    {
        // Optional
        public string folder_id { get; set; }
        public string project_id { get; set; }
        public int last_sync_sequence_num { get; set; } = -1;
        // Required
        public string user_device_id { get; set; }
        public string customer_id { get; set; }
        public string office_id { get; set; }
    }
}
