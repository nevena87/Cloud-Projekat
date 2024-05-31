using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class Login : TableEntity
    {
        public string Email { get; set; }
        public string Lozinka { get; set; }

        public Login(String indexNo)
        {
            PartitionKey = "login";
            RowKey = indexNo;
        }

        public Login() { }
    }
}
