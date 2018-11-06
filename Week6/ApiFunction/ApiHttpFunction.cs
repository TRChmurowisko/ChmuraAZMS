using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace TR.Test
{
    public static class ApiHttpFunction
    {
        [FunctionName("ApiHttpFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dateStr = req.Query["date"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            dateStr = dateStr ?? data?.dateStr;

            if (dateStr != null)
                return GetByDate(dateStr);
            else
                return new BadRequestObjectResult("Please pass a date (pl format) on the query string or in the request body");
        }

        private static ActionResult GetByDate(string dateStr)
        {
            DateTime dt;
            if (!DateTime.TryParse(dateStr, out dt))
            {
                return new BadRequestObjectResult("Failed to convert a date from string: " + dateStr);
            }
            // List<Message> list = new List<Message>();

            // list.Add(new Message() { Id = 1, AddedAt = new DateTime(2018, 11, 5), Content = "Here is the content" });

            IEnumerable<Message> list;
            var connString = GetDbConnectionString();
            try
            {
                using (var db = new StorageContext(connString))
                {
                    list = db.Messages.
                                Where(m => m.AddedAt.Date.Equals(dt.Date)).
                                ToList();
                }
                return new OkObjectResult(list);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }


        }

        private static string GetDbConnectionString()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", true)
            .Build();

            string s = "";
            if (configuration != null)
            {
                s = configuration["SQLConnectionString"];
            }
            return s;
        }
    }

    public class StorageContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        private string connectionString;

        public StorageContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    public class Message
    {
        public int Id { get; set; }

        public DateTime AddedAt { get; set; }

        public string Content { get; set; }

    }
}
