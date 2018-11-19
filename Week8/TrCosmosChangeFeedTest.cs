using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TR.CosmosTest
{
    public static class TrCosmosChangeFeedTest
    {
        [FunctionName("TrCosmosChangeFeedTest")]
        public static void Run([CosmosDBTrigger(
            databaseName: "CosmosTest",
            collectionName: "TemperatureFeed",
            ConnectionStringSetting = "w8test_DOCUMENTDB",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
