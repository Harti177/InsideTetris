using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Cosmos.Fluent;

namespace HartiCreations.InsideTetris
{
    public class SetHighScores
    {
        [Function("SetHighScores")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            CosmosClient cosmosClient = new CosmosClient(Program.EndpointUrl, Program.AuthorizationKey,  new CosmosClientOptions()
    	        {ApplicationRegion = Regions.WestEurope});
            await QueryItemsAsync(cosmosClient, req.Body);
            return new OkObjectResult(true);
        }

        public static class Program
        {
            public const string EndpointUrl = "https://insidetetriscosmosacc.documents.azure.com:443/";
            public const string AuthorizationKey = "AztWyHfFzKppABTTxZaWxc6fEvvG0T24CrFTlKCN85kZKR6TX0qrMbSMyO0Wm9LZ6GZ9Jx53qilyACDb763e9w==";
            public const string DatabaseId = "insidetetrisdb";
            public const string ContainerId = "highscores";
        }

        public class HighScoreDocument {
            public string id { get; set; }
            public List<HighScore> highscores { get; set; }
        }

        public class HighScore
        {
            public string userName { get; set; }
            
            public string userPassword { get; set; }

            public int userScore { get; set; }
        }

        private static async Task<bool> QueryItemsAsync(CosmosClient cosmosClient, Stream stream)
        {
            string body =  await new StreamReader(stream).ReadToEndAsync();

            dynamic json = JsonConvert.DeserializeObject<List<HighScore>>(body);

            var highscores = json;

            Container container = cosmosClient.GetContainer(Program.DatabaseId, Program.ContainerId);

            HighScoreDocument highScoreDocument = new HighScoreDocument
                {
                    //id = System.Guid.NewGuid().ToString(),
                    id = "6feffb12-5f66-4b25-a793-eb3d3e31774e",
                    highscores = highscores
                };

            await container.UpsertItemAsync<HighScoreDocument>(highScoreDocument); 

            return true;
        }
    }
}