using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

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

        /// prati se evidencija ko je upvote a ko downvote
        public string EmailKorisnikaUpvote { get; set; } = "";
        public string EmailKorisnikaDownvote { get; set; } = "";
    }
}
