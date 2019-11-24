using System;
using System.Collections.Generic;

namespace LawIT.Models.LawITContextModels
{
    public partial class DocumentWord
    {
        public int DocumentWordId { get; set; }
        public int DocumentId { get; set; }
        public int WordId { get; set; }
        public int Count { get; set; }

        public virtual Document Document { get; set; }
        public virtual Word Word { get; set; }
    }
}
