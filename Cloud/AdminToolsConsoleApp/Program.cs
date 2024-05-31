using KorisnikService_Data.Models;
using KorisnikService_Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AdminToolsConsoleApp
{
    public class Program
    {
        private static List<Administrator> admins = new List<Administrator>();

        public static async Task Main()
        {
            // dodavanje test admina
            await new AdministratorRepository().InsertAdministratorAsync(new Administrator("admin", "admin@mailinator.com", "123"));

            try
            {
                while (true)
                {
                    admins = await new AdministratorRepository().ReadAdministratorsAsync();

                    // Autentifikacija
                    Console.Write("Email: ");
                    string email = Console.ReadLine();
                    Console.Write("Lozinka: ");
                    string password = Console.ReadLine();

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        continue;
                    }
                    else
                    {
                        if (admins.FirstOrDefault(x => x.Email == email && x.Lozinka == password) != null)
                        {
                            Console.WriteLine("Uspesno ste se prijavili!\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Uneti su nevazeci podaci!\n");
                            continue;
                        }
                    }
                }

                while (true)
                {
                    admins = await new AdministratorRepository().ReadAdministratorsAsync();

                    Console.WriteLine("1) Dodaj administratora");
                    Console.WriteLine("2) Prikazi administratore");
                    Console.WriteLine("3) Ukloni administratora po Guid-u");
                    Console.WriteLine("q) Izlaz");
                    Console.Write("Unesite vas izbor: ");
                    string res = Console.ReadLine();
                    Console.WriteLine();

                    if (res == "q")
                        break;
                    else if (res == "1")
                    {
                        Console.WriteLine("Unesite detalje o administratoru:");
                        Console.Write("Ime: ");
                        string name = Console.ReadLine();
                        Console.Write("Email: ");
                        string email = Console.ReadLine();
                        Console.Write("Lozinka: ");
                        string password = Console.ReadLine();

                        Administrator newAdmin = new Administrator(name, email, password);
                        if ((await new AdministratorRepository().ReadAdministratorAsync(email) != null))
                        {
                            Console.WriteLine("Email je vec u upotrebi!\n");
                            continue;
                        }
                        else
                        {
                            if ((await new AdministratorRepository().InsertAdministratorAsync(newAdmin) != null))
                            {
                                Console.WriteLine("Administrator je uspesno dodat!\n");
                            }
                            else
                            {
                                Console.WriteLine("Greska pri dodavanju administratora!\n");
                            }
                        }
                    }
                    else if (res == "2")
                    {
                        foreach (Administrator admin in admins)
                        {
                            Console.WriteLine(admin);
                        }
                    }
                    else if (res == "3")
                    {
                        foreach (Administrator admin in admins)
                        {
                            Console.WriteLine(admin);
                        }
                        Console.Write("Unesite Guid za pocetak procesa brisanja: ");
                        string id = Console.ReadLine();

                        if (string.IsNullOrEmpty(id))
                        {
                            Console.WriteLine($"Administrator {id} ne postoji");
                        }

                        string rowKeyDeletion = admins.Where(a => a.Id == id).FirstOrDefault()?.RowKey;

                        if (rowKeyDeletion != null)
                        {
                            if ((await new AdministratorRepository().DeleteAdministratorAsync(rowKeyDeletion)))
                            {
                                Console.WriteLine("Administrator je uspesno obrisan!\n");
                            }
                            else
                            {
                                Console.WriteLine($"Administrator {id} ne postoji");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Doslo je do greske. Aplikacija je u greskom stanju!\n");
                Console.Write("Pritisnite bilo koji taster za zatvaranje aplikacije...");
                Console.ReadKey();
            }
        }
    }
}
