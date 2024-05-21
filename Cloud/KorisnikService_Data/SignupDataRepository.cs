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
    public class SignupDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private CloudBlobContainer _blobContainer;

        public SignupDataRepository()
        {
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=amdemostorage001;AccountKey=pF4stKY0yQ8/0uIUvt0qL4l5HLVfph1sEw8FnoBxYOdXGv/94QkN+FTlPmwXtdYI6Pzf7bjwWNZf+AStFiLqbQ==;EndpointSuffix=core.windows.net";
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("signup");
            CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference("slike");
            _blobContainer.CreateIfNotExists();
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
                existingSignup.Lozinka = updatedSignup.Lozinka;
               
                if (newImageFile != null && newImageFile.ContentLength > 0)
                {
                    
                    if (!string.IsNullOrEmpty(existingSignup.Slika))
                    {
                        Uri oldImageUri = new Uri(existingSignup.Slika);
                        string oldBlobName = oldImageUri.Segments.Last();
                        CloudBlockBlob oldBlockBlob = _blobContainer.GetBlockBlobReference(oldBlobName);
                        oldBlockBlob.DeleteIfExists();
                    }

                   
                    string imageName = Path.GetFileName(newImageFile.FileName);
                    CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(imageName);
                    blockBlob.UploadFromStream(newImageFile.InputStream);
                    existingSignup.Slika = blockBlob.Uri.ToString();
                }

                
                TableOperation updateOperation = TableOperation.Replace(existingSignup);
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
            signup.RowKey = Guid.NewGuid().ToString();
            signup.PartitionKey = "signup";
            signup.Ime = signup.Ime;
            signup.Prezime = signup.Prezime;
            signup.Adresa = signup.Adresa;
            signup.Grad = signup.Grad;
            signup.Drzava = signup.Drzava;
            signup.BrojTelefona = signup.BrojTelefona;
            signup.Email = signup.Email;
            signup.Lozinka = signup.Lozinka;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string imageName = Path.GetFileName(imageFile.FileName);
                CloudBlockBlob blockBlob = _blobContainer.GetBlockBlobReference(imageName);
                blockBlob.Properties.ContentType = imageFile.ContentType;
                blockBlob.UploadFromStream(imageFile.InputStream);
                signup.Slika = blockBlob.Uri.ToString();
            }

            TableOperation insertOperation = TableOperation.InsertOrMerge(signup);
            _table.Execute(insertOperation);
        }
    }
}
