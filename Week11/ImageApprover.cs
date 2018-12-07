using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace TRTest
{
    public static class ImageApprover
    {
        [FunctionName("ImageApprover")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string imageUrl = req.Query["imageUrl"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            imageUrl = imageUrl ?? data?.imageUrl;

            if (String.IsNullOrEmpty(imageUrl))
                new BadRequestObjectResult("Please pass a imageUrl in the request body");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();


            bool result = await IsAllowed(imageUrl, config["VisionService"], config["VisionServiceKey"], log);

            return result 
                ? (ActionResult)new OkObjectResult($"Picture from {imageUrl} allowed ")
                : new BadRequestObjectResult("This type of pictures is not allowed");
        }

        private static async Task<bool> IsAllowed(string imageUrl, string visionEndpoint, string serviceKey, ILogger log)
        {
            ApiKeyServiceClientCredentials credentials = new  ApiKeyServiceClientCredentials(serviceKey);

            using (var client = new ComputerVisionClient(credentials) { Endpoint = visionEndpoint  })
            {
                log.LogTrace("ComputerVisionClient is created");

                //
                // Analyze the URL for all visual features.
                //
                log.LogTrace("Calling ComputerVisionClient.AnalyzeImageAsync()...");

                VisualFeatureTypes[] visualFeatures = new VisualFeatureTypes[]
                { 
                    VisualFeatureTypes.Description, 
                    VisualFeatureTypes.Categories, 
                    VisualFeatureTypes.Adult, 
                    VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Tags};

                string language = "en";

                ImageAnalysis analysisResult = await client.AnalyzeImageAsync(imageUrl, visualFeatures, null, language);
                
                log.LogTrace("Checking analysis results");
                foreach (var tag in analysisResult.Description.Tags)
                {
                    if (tag.Equals("weapon"))
                        return false;
                }
                // analysisResult;
            }
            return true;
        }
    }
}
