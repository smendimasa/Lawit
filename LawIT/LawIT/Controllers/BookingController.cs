using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;

namespace LawIT.Controllers
{
    [Route("api /[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
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
            double totalAmount = 0; double totalNights = 0; double totalPersons = 0;
            if (request.QueryResult.Action == "book")
            {
                //Parse the intent params
                var requestParameters = request.QueryResult.Parameters;
                totalPersons = requestParameters.Fields["totalPersons"].NumberValue;
                totalNights = requestParameters.Fields["totalNights"].NumberValue;
                totalAmount = totalNights * 100;
            }

            // Populate the response
            WebhookResponse response = new WebhookResponse { FulfillmentText = $"Thank you for choosing our hotel, your total amount for the {totalNights} nights for {totalPersons} persons will be {totalAmount} USD." };
            // Ask Protobuf to format the JSON to return.
            // Again, we don’t want to use Json.NET — it doesn’t know how to handle Struct
            // values etc.
            string responseJson = response.ToString(); 
            return Content(responseJson, "application/json");
        }
    }
}