using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace KorisnikService_Data
{
    public class HealthStatus : TableEntity
    {
        public bool IsRedditAvailable { get; set; }

        public bool IsNotificationAvailable { get; set; }

        public HealthStatus()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "HealthStatus";
        }

        public HealthStatus(bool reddit, bool notification)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "HealthStatus";
            IsRedditAvailable = reddit;
            IsNotificationAvailable = notification;
        }
    }
}
