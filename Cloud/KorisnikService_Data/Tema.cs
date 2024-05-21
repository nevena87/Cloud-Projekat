using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KorisnikService_Data
{
    public class Tema : TableEntity
    {
        public Tema(string indexNo)
        {
            PartitionKey = "teme";
            RowKey = indexNo;
        }

        public Tema() { }

        public string Naslov { get; set; }
        public string Sadrzaj { get; set; }
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        public List<Komentar> Komentari { get; set; } = new List<Komentar>();
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public string UserEmail { get; set; }
        public string SlikaUrl { get; set; }
    }
}
