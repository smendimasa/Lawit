using System;
using System.Collections.Generic;

namespace LawIT.Models.LawITContextModels
{
    public partial class Subtitle
    {
        public int SubtitleId { get; set; }
        public string SubtitleName { get; set; }
        public string SubtitleNumber { get; set; }
        public int TitleId { get; set; }

        public virtual Title Title { get; set; }
    }
}
