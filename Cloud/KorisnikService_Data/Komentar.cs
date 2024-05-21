using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class Komentar : TableEntity
    {
        public string TemaId { get; set; }
        public string Tekst { get; set; } // Tekst komentara
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;// Datum kreiranja komentara
        public string KorisnikId { get; set; } // ID korisnika koji je ostavio komentar

        public Komentar(string indexNo)
        {
            PartitionKey = "komentari";
            RowKey = indexNo;
        }

        public Komentar()
        {
        }
    }
}
