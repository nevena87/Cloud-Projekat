using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Threading.Tasks;

namespace KorisnikService_Data.Queues
{
    public class CloudQueueHelper
    {
        private static CloudQueueClient _queueClient;

        static CloudQueueHelper()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            _queueClient = storageAccount.CreateCloudQueueClient();
        }

        public static CloudQueue GetQueue(string queueName)
        {
            CloudQueue queue = _queueClient.GetQueueReference(queueName.Trim().ToLower());
            queue.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            return queue;
        }

        public static async Task<bool> AddToQueue(CloudQueue queue, string content)
        {
            try
            {
                CloudQueueMessage queueMessage = new CloudQueueMessage(content);
                await queue.AddMessageAsync(queueMessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<CloudQueueMessage> ReadMessage(CloudQueue queue)
        {
            try
            {
                CloudQueueMessage message = await queue.GetMessageAsync();
                return message;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> DeleteMessage(CloudQueue queue, CloudQueueMessage messageId)
        {
            try
            {
                await queue.DeleteMessageAsync(messageId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
