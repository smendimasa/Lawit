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
        private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        // A Protobuf JSON parser configured to ignore unknown fields. This makes
        // the action robust against new fields being introduced by Dialogflow.
        

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
                titleId = requestParameters.Fields["title"].NumberValue;
            }
            if(inputString.Trim() != "")
            {
                documents = Helpers.Search(inputString, titleId.HasValue ? (int)titleId.Value : (int?)null);
            }
            var firstdocument = documents.FirstOrDefault();
            string responseText = firstdocument != null ? Helpers.ResponseBuilder(firstdocument) : "Sorry, I could not find a relevant document.";
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

    }
}
