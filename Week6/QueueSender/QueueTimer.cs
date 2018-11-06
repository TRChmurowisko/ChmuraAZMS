using System;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
// using Microsoft.WindowsAzure.Storage; 
// using Microsoft.WindowsAzure.Storage.Queue; 

namespace TR.Test
{
    public static class QueueTimer
    {
        [FunctionName("QueueTimer")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, 
                        ILogger log,
                        [Queue("stq-test",Connection = "AzureWebJobsStorage")] 
                        out string queueMessage)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            queueMessage = $"New message created at {DateTime.UtcNow.ToString()} utc";
            log.LogInformation($"C# Message created at: {DateTime.Now}");
        }
    }
}
