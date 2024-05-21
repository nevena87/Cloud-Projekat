using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KorisnikService_Data
{
    public class TemaRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private CloudBlobContainer _blobContainer;

        public TemaRepository()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=amdemostorage001;AccountKey=pF4stKY0yQ8/0uIUvt0qL4l5HLVfph1sEw8FnoBxYOdXGv/94QkN+FTlPmwXtdYI6Pzf7bjwWNZf+AStFiLqbQ==;EndpointSuffix=core.windows.net";
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("teme");
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("slike");
            _blobContainer.CreateIfNotExists();
        }

        public void AddTema(Tema tema, HttpPostedFileBase imageFile)
        {
            tema.RowKey = Guid.NewGuid().ToString();
            tema.PartitionKey = "teme";

            if (imageFile != null && imageFile.ContentLength > 0)
            {
               
                string imageName = Path.GetFileName(imageFile.FileName);
                CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(imageName);
                blockBlob.UploadFromStream(imageFile.InputStream);

               
                tema.SlikaUrl = blockBlob.Uri.ToString();
            }

            TableOperation insertOperation = TableOperation.Insert(tema);
            _table.Execute(insertOperation);
        }

        public void DeleteTema(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Tema>(partitionKey, rowKey);
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            Tema tema = (Tema)retrievedResult.Result;

            if (tema != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(tema);
                _table.Execute(deleteOperation);
            }
        }

        public IEnumerable<Tema> GetAllTeme()
        {
            TableQuery<Tema> query = new TableQuery<Tema>();
            return _table.ExecuteQuery(query);
        }

        public Tema GetTemaById(string id)
        {
            TableQuery<Tema> query = new TableQuery<Tema>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));
            return _table.ExecuteQuery(query).FirstOrDefault();
        }

        public void UpdateTema(Tema tema)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Tema>(tema.PartitionKey, tema.RowKey);
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            Tema existingTema = (Tema)retrievedResult.Result;

            if (existingTema != null)
            {
                existingTema.Naslov = tema.Naslov;
                existingTema.Sadrzaj = tema.Sadrzaj;
                existingTema.DatumKreiranja = tema.DatumKreiranja;
                existingTema.Upvotes = tema.Upvotes;
                existingTema.Downvotes = tema.Downvotes;
                existingTema.UserEmail = tema.UserEmail;

                TableOperation updateOperation = TableOperation.Replace(existingTema);
                _table.Execute(updateOperation);
            }
        }

        public IEnumerable<Tema> GetTemeByUserEmail(string userEmail)
        {
            var filter = TableQuery.GenerateFilterCondition("UserEmail", QueryComparisons.Equal, userEmail);
            TableQuery<Tema> query = new TableQuery<Tema>().Where(filter);
            return _table.ExecuteQuery(query);
        }
    }
}
