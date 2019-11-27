using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LawIT.Models.LawITContextModels;
using Microsoft.EntityFrameworkCore;
using LawIT.BLL;
using LawIT.Models.CustomClasses;
using Lucene.Net.Analysis;
using Lucene.Net.Util;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Tartarus.Snowball.Ext;

namespace LawIT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly LawITContext _context;
        public ValuesController(LawITContext context)
        {
            _context = context;

        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public List<DocumentResult> Search(string input, int? subtitleId, int? titleId)
        {
            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var tokens = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim(punctuation)).Distinct();
            var cleanedTokens = tokens.Where(x => !BLL.Constants.stopwords.Contains(x.ToLower())).ToList();
            var stemmedTokens = new List<string>();
            PorterStemmer stem = new PorterStemmer();

            foreach (var word in cleanedTokens)
            {
                stem.SetCurrent(word);
                stem.Stem();
                var result = stem.Current;
                stemmedTokens.Add(result);
            }
            var words = stemmedTokens.Distinct();

            // Get all word ids of cleaned token list
            var wordIds = _context.Word.Where(x => words.Contains(x.Word1)).Select(x => x.WordId).ToList();
            // Generate list od DocumentIds based on words and get the top 10
            var documentIds = _context.DocumentWord.Where(x => wordIds.Contains(x.WordId)).GroupBy(g => g.DocumentId).Select(y => new
            {
                DocumentId = y.Key,
                Counts = y.Sum(x => x.Count)
            }).OrderByDescending(c => c.Counts).Take(10).Select(x => x.DocumentId).ToList();
            List<int> filteredDocs = new List<int>();

            if (subtitleId != null)
            {
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId) && x.SubtitleId.HasValue && x.SubtitleId.Value == subtitleId).Select(x=> x.DocumentId).ToList();
                    
            }
            else if (titleId != null)
            {
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId) && x.TitleId == titleId).Select(x => x.DocumentId).ToList();
            }
            else
            {
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId)).ToList().Select(x => x.DocumentId).ToList();
            }
            var subtitles = _context.Subtitle.Select(x => new { x.SubtitleId, x.SubtitleName, x.SubtitleNumber }).ToDictionary(x => x.SubtitleId, x => new { x.SubtitleName, x.SubtitleNumber});
            List<DocumentResult> documents = _context.Document.Where(x=> filteredDocs.Contains(x.DocumentId)).Include(j => j.Title).Select(y => new DocumentResult
            {
                DocumentText = y.DocumentText,
                SubtitleName = y.SubtitleId.HasValue ? subtitles[y.SubtitleId.Value].SubtitleName : "",
                SubtitleNumber = y.SubtitleId.HasValue ? subtitles[y.SubtitleId.Value].SubtitleNumber : "",
                TitleName = y.Title.TitleName,
                TitleNumber = y.Title.TitleNumber
            }).ToList();
            return documents;
        }

    }
}
