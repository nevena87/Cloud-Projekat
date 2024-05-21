using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class Signup : TableEntity
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public string Grad { get; set; }
        public string Drzava { get; set; }
        public string BrojTelefona { get; set; }
        public string Email { get; set; }
        public string Lozinka { get; set; }
        public string Slika { get; set; }

        public Signup(String indexNo)
        {
            PartitionKey = "signup";
            RowKey = indexNo;
        }

        public Signup() { }
    }
}
