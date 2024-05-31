using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace KorisnikService_Data.Models
{
    public class Administrator : TableEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Lozinka { get; set; }

        public Administrator()
        {
            PartitionKey = "Administrator";
            Id = Guid.NewGuid().ToString().Replace("-", "");
        }

        public Administrator(string name, string email, string lozinka)
        {
            PartitionKey = "Administrator";
            RowKey = email;
            Id = Guid.NewGuid().ToString().Replace("-", "");
            Name = name;
            Email = email;
            Lozinka = lozinka;
        }

        public override string ToString()
        {
            return $"`````````````````````````````````````````````````````\n" +
                   $"ID       : {Id}\n" +
                   $"Name     : {Name}\n" +
                   $"Email    : {Email}\n" +
                   $"Password : {Lozinka}\n" +
                   "`````````````````````````````````````````````````````\n";
        }
    }
}
