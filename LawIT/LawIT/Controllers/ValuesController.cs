using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LawIT.Models.LawITContextModels;
using Microsoft.EntityFrameworkCore;
using LawIT.BLL;

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

        public void Search(string input)
        {
            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var tokens = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim(punctuation)).Distinct();
            var cleanedTokens = tokens.Where(x => !Constants.stopwords.Contains(x)).ToList();
            var stemmedTokens = cleanedTokens;

            var words = new List<string>();

            var wordIds = _context.Word.Where(x => words.Contains(x.Word1)).Select(x => x.WordId).ToList();
            var documentIds = _context.DocumentWord.Where(x => wordIds.Contains(x.WordId)).GroupBy(g => g.DocumentId).Select(y => new
            {
                DocumentId = y.Key,
                Counts = y.Sum(x => x.Count)
            }).OrderByDescending(c => c.Counts).Take(10).Select(x => x.DocumentId).ToList();
            var docs = _context.Document.Where(x => documentIds.Contains(x.DocumentId)).Include(i => i.Subtitle).ThenInclude(j => j.Title).Select(y => new
            {
                y.DocumentText,
                y.Subtitle.SubtitleName,
                y.Subtitle.SubtitleNumber,
                y.Subtitle.Title.TitleName,
                y.Subtitle.Title.TitleNumber
            }).ToList();

        }
        public void SearchWithinSubtitle(string input, int subtitleId)
        {
            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = input.Split().Select(x => x.Trim(punctuation)).Distinct();

            var wordIds = _context.Word.Where(x => words.Contains(x.Word1)).Select(x => x.WordId).ToList();
            var documentIds = _context.DocumentWord.Include(i => i.Document.SubtitleId).Where(x => wordIds.Contains(x.WordId) && x.Document.SubtitleId == subtitleId)
                .GroupBy(g => g.DocumentId).Select(y => new
                {
                    DocumentId = y.Key,
                    Counts = y.Sum(x => x.Count)
                }).OrderByDescending(c => c.Counts).Take(10).Select(x => x.DocumentId).ToList();
            var docs = _context.Document.Where(x => documentIds.Contains(x.DocumentId)).Include(i => i.Subtitle).ThenInclude(j => j.Title)
                .Select(y => new
                {
                    y.DocumentText,
                    y.Subtitle.SubtitleName,
                    y.Subtitle.SubtitleNumber,
                    y.Subtitle.Title.TitleName,
                    y.Subtitle.Title.TitleNumber
                }).ToList();

        }
        public void SearchWithinTitle(string input, int titleId)
        {
            var punctuation = input.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = input.Split().Select(x => x.Trim(punctuation)).Distinct();

            var wordIds = _context.Word.Where(x => words.Contains(x.Word1)).Select(x => x.WordId).ToList();
            var documentIds = _context.DocumentWord.Include(i => i.Document.Subtitle.TitleId).Where(x => wordIds.Contains(x.WordId) && x.Document.Subtitle.TitleId == titleId)
                .GroupBy(g => g.DocumentId).Select(y => new
                {
                    DocumentId = y.Key,
                    Counts = y.Sum(x => x.Count)
                }).OrderByDescending(c => c.Counts).Take(10).Select(x => x.DocumentId).ToList();
            var docs = _context.Document.Where(x => documentIds.Contains(x.DocumentId)).Include(i => i.Subtitle).ThenInclude(j => j.Title)
                .Select(y => new
                {
                    y.DocumentText,
                    y.Subtitle.SubtitleName,
                    y.Subtitle.SubtitleNumber,
                    y.Subtitle.Title.TitleName,
                    y.Subtitle.Title.TitleNumber
                }).ToList();

        }
    }
}
