using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;

namespace TR.Test
{
    public static class QueuePicker
    {
        [FunctionName("QueuePicker")]
        public static void Run([QueueTrigger("stq-test", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"Message picked from the queue up: {myQueueItem}");
            if (!String.IsNullOrEmpty(myQueueItem))
            {
                log.LogInformation($"Adding to db");
                AddToDb(myQueueItem, log);
                log.LogInformation($"Added");
            }
            else
                log.LogInformation("Empty message, skipped");
        }

        private static void AddToDb(string myQueueItem, ILogger log)
        {
            var connString = GetDbConnectionString();
            //log.LogInformation($"cs={connString}");

            using (var db = new StorageContext(connString))
            {
                db.Messages.Add(new Message { AddedAt = DateTime.Now, Content = myQueueItem });
                var count = db.SaveChanges();
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
