using KorisnikService_Data.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService_Data
{
    public class NotificationRepository
    {
        private CloudTable table;

        public NotificationRepository()
        {
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("notifications");
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<List<Notification>> ReadNotificationsAsync()
        {
            if (table == null)
                return null;

            TableQuery<Notification> query = new TableQuery<Notification>();
            TableContinuationToken continuationToken = null;
            var notifications = new List<Notification>();

            do
            {
                TableQuerySegment<Notification> queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                notifications.AddRange(queryResult.Results);
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return notifications.Take(500).ToList();
        }

        public async Task<Notification> InsertNotificationAsync(Notification notification)
        {
            if (table == null || notification == null)
                return null;

            TableOperation insertOperation = TableOperation.InsertOrMerge(notification);
            return (await table.ExecuteAsync(insertOperation)).Result as Notification;
        }
    }
}