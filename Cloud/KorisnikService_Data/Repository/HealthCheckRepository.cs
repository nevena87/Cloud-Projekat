using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KorisnikService_Data
{
    public class HealthCheckRepository
    {
        private CloudTable table;

        public HealthCheckRepository()
        {
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("HealthCheck");
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<List<HealthStatus>> ReadStatusesAsync()
        {
            if (table == null)
                return null;

            TableQuery<HealthStatus> query = new TableQuery<HealthStatus>();
            TableContinuationToken continuationToken = null;
            var statuses = new List<HealthStatus>();

            do
            {
                TableQuerySegment<HealthStatus> queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                statuses.AddRange(queryResult.Results);
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return statuses.Take(500).ToList();
        }

        public async Task<HealthStatus> InsertStatusAsync(HealthStatus status)
        {
            if (table == null || status == null)
                return null;

            TableOperation insertOperation = TableOperation.InsertOrMerge(status);
            return (await table.ExecuteAsync(insertOperation)).Result as HealthStatus;
        }
    }
}
