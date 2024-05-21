using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class PretplataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public PretplataRepository()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=amdemostorage001;AccountKey=pF4stKY0yQ8/0uIUvt0qL4l5HLVfph1sEw8FnoBxYOdXGv/94QkN+FTlPmwXtdYI6Pzf7bjwWNZf+AStFiLqbQ==;EndpointSuffix=core.windows.net";
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("pretplate");
        }

        public void AddPretplata(Pretplata pretplata)
        {
            TableOperation insertOperation = TableOperation.Insert(pretplata);
            _table.Execute(insertOperation);
        }

        public void DeletePretplata(string userEmail, string temaId)
        {
            
            var retrieveOperation = TableOperation.Retrieve<Pretplata>(userEmail, temaId);
            var retrievedResult = _table.Execute(retrieveOperation);
            Pretplata pretplata = (Pretplata)retrievedResult.Result;

            if (pretplata != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(pretplata);
                _table.Execute(deleteOperation);
            }
        }

        public IEnumerable<Pretplata> GetPretplateByUserEmail(string userEmail)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userEmail);
            TableQuery<Pretplata> query = new TableQuery<Pretplata>().Where(filter);
            return _table.ExecuteQuery(query);
        }

        public bool IsPretplacen(string userEmail, string temaId)
        {
            var retrieveOperation = TableOperation.Retrieve<Pretplata>(userEmail, temaId);
            var retrievedResult = _table.Execute(retrieveOperation);

            return retrievedResult.Result != null;
        }
    }
}

