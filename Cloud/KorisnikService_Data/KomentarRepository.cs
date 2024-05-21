
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class KomentarRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public KomentarRepository()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=amdemostorage001;AccountKey=pF4stKY0yQ8/0uIUvt0qL4l5HLVfph1sEw8FnoBxYOdXGv/94QkN+FTlPmwXtdYI6Pzf7bjwWNZf+AStFiLqbQ==;EndpointSuffix=core.windows.net";
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("komentari");
        }

        public void AddKomentar(Komentar komentar)
        {
            // Postavite PartitionKey i RowKey za komentar
            komentar.PartitionKey = "komentari";
            komentar.RowKey = Guid.NewGuid().ToString();

            // Izvršite operaciju umetanja u tabelu
            TableOperation insertOperation = TableOperation.Insert(komentar);
            _table.Execute(insertOperation);
        }

        public void DeleteKomentar(string partitionKey, string rowKey)
        {
            // Pronađite komentar koji treba obrisati
            TableOperation retrieveOperation = TableOperation.Retrieve<Komentar>(partitionKey, rowKey);
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            Komentar komentar = (Komentar)retrievedResult.Result;

            // Obrišite komentar ako postoji
            if (komentar != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(komentar);
                _table.Execute(deleteOperation);
            }
        }

        public IEnumerable<Komentar> GetAllKomentari()
        {
            // Izvršite upit koji će vratiti sve komentare
            TableQuery<Komentar> query = new TableQuery<Komentar>();
            return _table.ExecuteQuery(query);
        }

        public IEnumerable<Komentar> GetKomentariByTemaId(string temaId)
        {
            // Izvršite upit koji će vratiti sve komentare za određenu temu
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
