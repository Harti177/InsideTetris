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

namespace HartiCreations.InsideTetris
{
    public class GetHighScores
    {
        [Function("GetHighScores")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            CosmosClient cosmosClient = new CosmosClient(Program.EndpointUrl, Program.AuthorizationKey,  new CosmosClientOptions()
    	        {ApplicationRegion = Regions.WestEurope});
            List<HighScore> highScores = await QueryItemsAsync(cosmosClient);
            return new OkObjectResult(highScores);
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

        private static async Task<List<HighScore>> QueryItemsAsync(CosmosClient cosmosClient)
        {
            var sqlQueryText = "SELECT * FROM c";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            Container container = cosmosClient.GetContainer(Program.DatabaseId, Program.ContainerId);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            List<HighScore> highScores = new List<HighScore>();

            FeedIterator<HighScoreDocument> feedIterator = container.GetItemQueryIterator<HighScoreDocument>(queryDefinition); 

            while (feedIterator.HasMoreResults)
                foreach (var item in await feedIterator.ReadNextAsync()){
                    return item.highscores; 
                }

            return highScores;
        }
    }
}
