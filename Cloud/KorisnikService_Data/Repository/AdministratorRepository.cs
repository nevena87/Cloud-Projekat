using KorisnikService_Data.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorisnikService_Data.Repository
{
    public class AdministratorRepository
    {
        private CloudTable table;

        public AdministratorRepository()
        {
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Administrators");
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<Administrator> ReadAdministratorAsync(string rowKey)
        {
            if (table == null || string.IsNullOrEmpty(rowKey))
                return null;

            TableOperation retrieveOperation = TableOperation.Retrieve<Administrator>("Administrator", rowKey);
            return (await table.ExecuteAsync(retrieveOperation)).Result as Administrator;
        }

        public async Task<Administrator> ReadAdministratorByIdAsync(string id)
        {
            if (table == null || string.IsNullOrEmpty(id))
                return null;

            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Administrator");
            filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, id));

            var query = new TableQuery<Administrator>().Where(filter);
            var queryResult = await table.ExecuteQuerySegmentedAsync(query, null);

            return queryResult.FirstOrDefault();
        }

        public async Task<List<Administrator>> ReadAdministratorsAsync()
        {
            if (table == null)
                return null;

            TableQuery<Administrator> query = new TableQuery<Administrator>();
            TableContinuationToken continuationToken = null;
            var administrators = new List<Administrator>();

            do
            {
                TableQuerySegment<Administrator> queryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                administrators.AddRange(queryResult.Results);
                continuationToken = queryResult.ContinuationToken;
            } while (continuationToken != null);

            return administrators;
        }

        public async Task<Administrator> InsertAdministratorAsync(Administrator administrator)
        {
            if (table == null || administrator == null)
                return null;

            TableOperation insertOperation = TableOperation.InsertOrMerge(administrator);
            return (await table.ExecuteAsync(insertOperation)).Result as Administrator;
        }

        public async Task<Administrator> UpdateAdministratorAsync(string oldRowKey, Administrator administrator)
        {
            if (table == null || administrator == null)
                return null;

            if (await DeleteAdministratorAsync(oldRowKey))
                return await InsertAdministratorAsync(administrator);
            else
                return null;
        }

        public async Task<bool> DeleteAdministratorAsync(string rowKey)
        {
            if (table == null || string.IsNullOrEmpty(rowKey))
                return false;

            Administrator administrator = await ReadAdministratorAsync(rowKey);

            if (administrator != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(administrator);
                return await table.ExecuteAsync(deleteOperation) != null;
            }
            else
                return false;
        }
    }
}
