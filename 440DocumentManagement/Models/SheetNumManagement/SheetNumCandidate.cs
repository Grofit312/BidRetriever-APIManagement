using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _440DocumentManagement.Models.SheetNumManagement
{
    public class SheetNumCandidate : BaseModel
    {
        public string actual_sheetnum { get; set; }
        public string candidate_id { get; set; }
        public int corrected_confidence { get; set; }
        public int corrected_word_pattern { get; set; }
        public string corrected_word_text { get; set; }
        //public DateTime create_datetime { get; set; }
        public string discipline { get; set; }
        public string discipline_class { get; set; }
        public string file_id { get; set; }
        public string filename { get; set; }
        public string match_font_type { get; set; }
        public string match_pattern { get; set; }
        public string match_text { get; set; }
        public string match_status { get; set; }
        public int match_x1 { get; set; }
        public int match_x2 { get; set; }
        public int match_y1 { get; set; }
        public int match_y2 { get; set; }
        public int original_confidence { get; set; }
        public int original_sequence { get; set; }
        public string original_word_pattern { get; set; }
        public string original_word_text { get; set; }
        public string project_id { get; set; }
        public string stripped_actual { get; set; }
        public int stripped_confidence { get; set; }
        public string stripped_word_pattern { get; set; }
        public string stripped_word_text { get; set; }
        public string test_number { get; set; }
        public int match_font_size { get; set; }
    }

    public class SheetNumCandidateFindRequest : BaseModel
    {
        public string file_id { get; set; }
        public string doc_id { get; set; }
    }
}
