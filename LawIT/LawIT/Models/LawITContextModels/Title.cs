using System;
using System.Collections.Generic;

namespace LawIT.Models.LawITContextModels
{
    public partial class Title
    {
        public Title()
        {
            Document = new HashSet<Document>();
            Subtitle = new HashSet<Subtitle>();
        }

        public int TitleId { get; set; }
        public string TitleName { get; set; }
        public string TitleNumber { get; set; }

        public virtual ICollection<Document> Document { get; set; }
        public virtual ICollection<Subtitle> Subtitle { get; set; }
    }
}
