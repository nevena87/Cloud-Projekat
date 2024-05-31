using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KorisnikService_Data
{
    public class LoginDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public LoginDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("signup");
            _table.CreateIfNotExists();
        }

        public IEnumerable<Login> RetrieveAllLogin()
        {
            TableQuery<Login> query = new TableQuery<Login>();
            return _table.ExecuteQuery(query);
        }

        public void AddLogin(Login login)
        {
            login.RowKey = Guid.NewGuid().ToString();
            login.PartitionKey = "login";
            login.Email = login.Email;
            login.Lozinka = login.Lozinka;
            TableOperation insertOperation = TableOperation.InsertOrMerge(login);
            _table.Execute(insertOperation);
        }

        public Login FindByEmail(string email)
        {
            var query = new TableQuery<Login>().Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));
            var result = _table.ExecuteQuery(query);
            return result.FirstOrDefault();
        }

        public bool ValidateLogin(string email, string password)
        {
            var user = FindByEmail(email);
            if (user != null && user.Lozinka == password)
            {
                return true;
            }
            return false;
        }
    }
}
