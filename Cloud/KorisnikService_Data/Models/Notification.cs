using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace KorisnikService_Data.Models
{
    public class Notification : TableEntity
    {

        public string CommentId { get; set; }

        public int EmailsSent { get; set; } = 0;

        public Notification()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "Notification";
        }

        public Notification(string id, int emailsSent)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "Comment";
            CommentId = id;
            EmailsSent = emailsSent;
        }
    }
}
