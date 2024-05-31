
using KorisnikService_Data.Queues;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class KomentarRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public KomentarRepository()
        {
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("komentari");
            _table.CreateIfNotExists();
        }

        public async Task AddKomentar(Komentar komentar)
        {

            komentar.PartitionKey = "komentari";
            komentar.RowKey = Guid.NewGuid().ToString();

            TableOperation insertOperation = TableOperation.Insert(komentar);
            _table.Execute(insertOperation);

            // Dodavanje komenatara u red cekanja
            await CloudQueueHelper.AddToQueue(CloudQueueHelper.GetQueue("notifications"), komentar.RowKey);
        }

        public void DeleteKomentar(string partitionKey, string rowKey)
        {

            TableOperation retrieveOperation = TableOperation.Retrieve<Komentar>(partitionKey, rowKey);
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            Komentar komentar = (Komentar)retrievedResult.Result;


            if (komentar != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(komentar);
                _table.Execute(deleteOperation);
            }
        }

        public IEnumerable<Komentar> GetAllKomentari()
        {

            TableQuery<Komentar> query = new TableQuery<Komentar>();
            return _table.ExecuteQuery(query);
        }

        public IEnumerable<Komentar> GetKomentariByTemaId(string temaId)
        {

            TableQuery<Komentar> query = new TableQuery<Komentar>().Where(TableQuery.GenerateFilterCondition("TemaId", QueryComparisons.Equal, temaId));
            return _table.ExecuteQuery(query);
        }

        public Komentar GetKomentarById(string id)
        {
            TableQuery<Komentar> query = new TableQuery<Komentar>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            return _table.ExecuteQuery(query).FirstOrDefault();
        }
    }
}
