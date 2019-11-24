using System;
using System.Collections.Generic;

namespace LawIT.Models.LawITContextModels
{
    public partial class Word
    {
        public Word()
        {
            DocumentWord = new HashSet<DocumentWord>();
        }

        public int WordId { get; set; }
        public string Word1 { get; set; }

        public virtual ICollection<DocumentWord> DocumentWord { get; set; }
    }
}
