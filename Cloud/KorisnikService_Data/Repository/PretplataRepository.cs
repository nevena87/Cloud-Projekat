using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace KorisnikService_Data
{
    public class PretplataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public PretplataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("pretplate");
            _table.CreateIfNotExists();
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

        public IEnumerable<Pretplata> GetPretplateByTemaId(string tema_id)
        {
            var filter = TableQuery.GenerateFilterCondition("TemaId", QueryComparisons.Equal, tema_id);
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

