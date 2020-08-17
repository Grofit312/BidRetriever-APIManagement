namespace _440DocumentManagement.Models.ProjectEvalCriteria
{
    public class ProjectEvalCriteria : BaseModel
    {
        public string action_attribute { get; set; }
        public string action_value { get; set; }
        public string condition_source { get; set; }
        public string condition_source_operator { get; set; }
        public string condition_source_value { get; set; }
        public string customer_id { get; set; }
        public string customer_office_id { get; set; }
        public string eval_criteria_id { get; set; }
    }
}
