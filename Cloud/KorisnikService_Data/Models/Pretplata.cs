using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class Pretplata : TableEntity
    {
        public Pretplata(string userEmail, string temaId)
        {
            PartitionKey = userEmail;
            RowKey = temaId;
        }

        public Pretplata() { }

        public string UserEmail
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string TemaId
        {
            get { return RowKey; }
            set { RowKey = value; }
        }
    }
}
