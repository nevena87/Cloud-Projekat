using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("teme");
            _table.CreateIfNotExists();
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("slike");
            _blobContainer.CreateIfNotExists();
            EnableCors(_blobContainer);
        }

        public void AddTema(Tema tema, HttpPostedFileBase imageFile)
        {
            tema.RowKey = Guid.NewGuid().ToString();
            tema.PartitionKey = "teme";

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string imageName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(imageName);
                blockBlob.Properties.ContentType = imageFile.ContentType;

                using (var stream = imageFile.InputStream)
                {
                    blockBlob.UploadFromStream(stream);
                }

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

                // brisanje komentara povezanih na temu
                KomentarRepository komentar = new KomentarRepository();

                var povezani_komentari = komentar.GetAllKomentari().Where(k => k.TemaId == rowKey); // samo povezani komenatari

                foreach (Komentar k in povezani_komentari)
                {
                    komentar.DeleteKomentar(k.PartitionKey, k.RowKey);
                }

                // brisanje pretplata za temu koja se brise
                PretplataRepository pretplataRepository = new PretplataRepository();

                var pretplate = pretplataRepository.GetPretplateByTemaId(rowKey).ToList();

                foreach (Pretplata p in pretplate)
                {
                    pretplataRepository.DeletePretplata(p.UserEmail, p.TemaId);
                }
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
                existingTema.EmailKorisnikaUpvote = tema.EmailKorisnikaUpvote;
                existingTema.EmailKorisnikaDownvote = tema.EmailKorisnikaDownvote;

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

        private static void EnableCors(CloudBlobContainer container)
        {
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            container.SetPermissions(permissions);

            var serviceProperties = container.ServiceClient.GetServiceProperties();
            serviceProperties.Cors.CorsRules.Clear();
            serviceProperties.Cors.CorsRules.Add(new CorsRule
            {
                AllowedOrigins = new[] { "*" },
                AllowedMethods = CorsHttpMethods.Get | CorsHttpMethods.Post | CorsHttpMethods.Put,
                AllowedHeaders = new[] { "*" },
                ExposedHeaders = new[] { "*" },
                MaxAgeInSeconds = 3600
            });
            container.ServiceClient.SetServiceProperties(serviceProperties);
        }
    }
}
