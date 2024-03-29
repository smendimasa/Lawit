﻿using System;
using System.Collections.Generic;

namespace LawIT.Models.LawITContextModels
{
    public partial class Document
    {
        public Document()
        {
            DocumentWord = new HashSet<DocumentWord>();
        }

        public int DocumentId { get; set; }
        public int TitleId { get; set; }
        public int? SubtitleId { get; set; }
        public string DocumentText { get; set; }
        public string DocumentHeader { get; set; }
        public string UniversalCitation { get; set; }

        public virtual Title Title { get; set; }
        public virtual ICollection<DocumentWord> DocumentWord { get; set; }
    }
}
