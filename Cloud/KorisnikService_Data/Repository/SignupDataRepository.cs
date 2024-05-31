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
    public class SignupDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private CloudBlobContainer _blobContainer;
        private CloudBlobClient _blobClient;

        public SignupDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("signup");
            _table.CreateIfNotExists(); // kreirati tabelu pre upotrebe
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference("slike");
            _blobContainer.CreateIfNotExists();
            EnableCors(_blobContainer);
        }

        public Signup RetrieveSignupByEmail(string email)
        {
            TableQuery<Signup> query = new TableQuery<Signup>().Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));
            var results = _table.ExecuteQuery(query);
            return results.FirstOrDefault();
        }

        public void UpdateSignup(Signup updatedSignup, HttpPostedFileBase newImageFile)
        {
            if (string.IsNullOrEmpty(updatedSignup.PartitionKey) || string.IsNullOrEmpty(updatedSignup.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<Signup>(updatedSignup.PartitionKey, updatedSignup.RowKey);
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            Signup existingSignup = (Signup)retrievedResult.Result;

            if (existingSignup != null)
            {
                existingSignup.Ime = updatedSignup.Ime;
                existingSignup.Prezime = updatedSignup.Prezime;
                existingSignup.Adresa = updatedSignup.Adresa;
                existingSignup.Grad = updatedSignup.Grad;
                existingSignup.Drzava = updatedSignup.Drzava;
                existingSignup.BrojTelefona = updatedSignup.BrojTelefona;
                existingSignup.Email = updatedSignup.Email;

                if (string.IsNullOrEmpty(updatedSignup.Lozinka))
                    existingSignup.Lozinka = "12345678";
                else
                    existingSignup.Lozinka = updatedSignup.Lozinka;

                if (newImageFile != null && newImageFile.ContentLength > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(newImageFile.FileName)}";
                    var blob = _blobContainer.GetBlockBlobReference(fileName);

                    using (var stream = newImageFile.InputStream)
                    {
                        blob.UploadFromStream(stream);
                    }

                    existingSignup.Slika = blob.Uri.ToString();
                }

                TableOperation updateOperation = TableOperation.InsertOrReplace(existingSignup);
                _table.Execute(updateOperation);
            }
        }

        public IEnumerable<Signup> RetrieveAllSignup()
        {
            TableQuery<Signup> query = new TableQuery<Signup>();
            return _table.ExecuteQuery(query);
        }

        public void AddSignup(Signup signup, HttpPostedFileBase imageFile)
        {
            // proveri da li je email zauzet
            var postoji = RetrieveSignupByEmail(signup.Email);

            // email je zauzet
            if (postoji != null)
                return;

            signup.RowKey = Guid.NewGuid().ToString();
            signup.PartitionKey = "signup";

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string imageName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(imageName);
                blockBlob.Properties.ContentType = imageFile.ContentType;

                using (var stream = imageFile.InputStream)
                {
                    blockBlob.UploadFromStream(stream);
                }

                signup.Slika = blockBlob.Uri.ToString();
            }

            TableOperation insertOperation = TableOperation.InsertOrMerge(signup);
            _table.Execute(insertOperation);
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
