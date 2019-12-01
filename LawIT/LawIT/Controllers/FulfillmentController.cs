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
using Google.Protobuf;
using Google.Cloud.Dialogflow.V2;
using System.IO;

namespace LawIT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FulfillmentController : ControllerBase
    {
        private readonly LawITContext _context;
        public FulfillmentController(LawITContext context)
        {
            _context = context;

        }

        // A Protobuf JSON parser configured to ignore unknown fields. This makes
        // the action robust against new fields being introduced by Dialogflow.
        private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        [HttpPost]
        public ContentResult DialogAction()
        {
            // Parse the body of the request using the Protobuf JSON parser,
            // *not* Json.NET.

            WebhookRequest request;
            using (var reader = new StreamReader(Request.Body))
            {
                request = jsonParser.Parse<WebhookRequest>(reader);
            }
            double? titleId = null;
            var inputString = request.QueryResult.QueryText;
            List<DocumentResult> documents = new List<DocumentResult>();
            if (request.QueryResult.Action == "title")
            {
                //Parse the intent params
                var requestParameters = request.QueryResult.Parameters;
                titleId = requestParameters.Fields["subtitle"].NumberValue;
            }
            if(inputString.Trim() != "")
            {
                documents = Search(inputString, titleId.HasValue ? (int)titleId.Value : (int?)null);
            }
            var firstdocument = documents.FirstOrDefault();
            string responseText = firstdocument != null ? ResponseBuilder(firstdocument) : "Sorry, I could not find a relevant document.";
            // Populate the response
            WebhookResponse response = new WebhookResponse
            {
                FulfillmentText = responseText
            };
            // Ask Protobuf to format the JSON to return.
            // Again, we don’t want to use Json.NET — it doesn’t know how to handle Struct
            // values etc.
            string responseJson = response.ToString();
            return Content(responseJson, "application/json");
        }
        public List<DocumentResult> Search(string input, int? titleId)
        {
            //  Take the input, split up into words while discarding symbols and numbers, then remove the stop words and set all cases to lowercase
            var tokens = input.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Where(c => char.IsLetter(c)).Aggregate("", (current, c) => current + c))
                .Select(x => x.ToLower()).Distinct()
                .Where(x => !BLL.Constants.stopwords.Contains(x));
            var stemmedTokens = new List<string>();
            // Instantiate the stemmer
            PorterStemmer stem = new PorterStemmer();
            // Stem all the words in the input and add to the list
            foreach (var word in tokens)
            {
                stem.SetCurrent(word);
                stem.Stem();
                var result = stem.Current;
                stemmedTokens.Add(result);
            }
            // just in case some words have common stems, we apply the Distinct filter again
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

            //if (subtitleId != null)
            //{
            //    filteredDocs = _context.Document.Where(x => documentIds.Contains(x.DocumentId) && x.SubtitleId.HasValue && x.SubtitleId.Value == subtitleId).Select(x=> x.DocumentId).ToList();
                    
            //}
            //else 
            if (titleId != null)
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
                TitleNumber = y.Title.TitleNumber,
                Citation = y.UniversalCitation,
                DocumentHeader = y.DocumentHeader
            }).ToList();
            return documents;
        }
        public string ResponseBuilder(DocumentResult document)
        {
            var TitleHeader = "Title " + document.TitleNumber + " - " + document.TitleName + Environment.NewLine;
            var SubtitleHeader = document.SubtitleNumber.Trim() != "" && document.SubtitleName.Trim() != "" ? 
                "Subtitle " + document.SubtitleNumber + " - " + document.SubtitleName + Environment.NewLine : "";
            var Body = document.DocumentHeader + Environment.NewLine + Environment.NewLine + document.DocumentText + Environment.NewLine + Environment.NewLine + document.Citation;
            return (TitleHeader + SubtitleHeader + Body);
        }
        #region boilerplate
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
        #endregion

    }
}
