using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LawIT.Models.LawITContextModels;
using Microsoft.EntityFrameworkCore;
using LawIT.BLL;
using LawIT.Models.CustomClasses;

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
            var cleanedTokens = tokens.Where(x => !Constants.stopwords.Contains(x.ToLower())).ToList();
            var stemmedTokens = cleanedTokens;

            var words = new List<string>();
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
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId) && x.SubtitleId == subtitleId).Select(x=> x.DocumentId).ToList();
                    
            }
            else if (titleId != null)
            {
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId) && x.Subtitle.TitleId == titleId).Select(x => x.DocumentId).ToList();
            }
            else
            {
                filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId)).ToList().Select(x => x.DocumentId).ToList();
            }
            List<DocumentResult> documents = _context.Document.Where(x=> filteredDocs.Contains(x.DocumentId)).Include(i => i.Subtitle).ThenInclude(j => j.Title).Select(y => new DocumentResult
            {
                DocumentText = y.DocumentText,
                SubtitleName = y.Subtitle.SubtitleName,
                SubtitleNumber = y.Subtitle.SubtitleNumber,
                TitleName = y.Subtitle.Title.TitleName,
                TitleNumber = y.Subtitle.Title.TitleNumber
            }).ToList();
            return documents;
        }
    }
}
