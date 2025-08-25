using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Domen.Klase;
using Domen.Enumeracije;
using System.Collections.Generic;
using System.Configuration;

namespace LekarSpecijalista
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6004; // Koristimo drugi port za lekara specijalistu

            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine("[LekarSpecijalista] Lekar specijalista sluša na portu 6004...");

                // Inicijalizacija Random klase samo jednom
                Random rand = new Random();

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("[LekarSpecijalista] Primljena konekcija.");

                    new Thread(() =>
                    {
                        try
                        {
                            // Povezivanje sa serverom za preuzimanje liste pacijenata
                            using (NetworkStream ns = client.GetStream())
                            {
                                BinaryFormatter formatter = new BinaryFormatter();

                                // Preuzimanje liste pacijenata sa servera
                                List<Pacijent> pacijenti = (List<Pacijent>)formatter.Deserialize(ns);
                                Console.WriteLine($"[LekarSpecijalista] Preuzeta lista pacijenata. Broj pacijenata: {pacijenti.Count}");

                                // Obrada urgentnih pacijenata (ako je to relevantno)
                                Console.WriteLine("[LekarSpecijalista] Obrada urgentnih pacijenata:");
                               /* foreach (var pacijent in pacijenti)
                                {
                                    if (pacijent.VrsteZahteva == VrsteZahteva.URGENTA_POMOC)
                                    {
                                        // Obrada pacijenata sa urgentnim zahtevima
                                        Console.WriteLine($"[LekarSpecijalista] Obrađujem URGENTNI zahtev za pacijenta {pacijent.Ime} {pacijent.Prezime}");
                                        Thread.Sleep(10000); // Simulacija vremena obrade

                                        StatusLekar noviStatus = rand.Next(2) == 0 ? StatusLekar.SPREMAN_ZA_ODLAZAK : StatusLekar.PONOVITI_OPERACIJU;
                                        // Oznaka pacijenta kao obradjenog
                                        AžurirajStatusPacijenta(pacijent.LBO, noviStatus);
                                        if (noviStatus == StatusLekar.SPREMAN_ZA_ODLAZAK)
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} je spreman da ide kući.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} mora ponoviti operaciju.");
                                        }
                                    }
                                }

                                // Obrada preostalih pacijenata
                                Console.WriteLine("[LekarSpecijalista] Obrada ostalih pacijenata:");
                                foreach (var pacijent in pacijenti)
                                {
                                    if (pacijent.VrsteZahteva == VrsteZahteva.TERAPIJA)
                                    {
                                        // Obrada pacijenata sa terapijama
                                        Console.WriteLine($"[LekarSpecijalista] Obrada pacijenta {pacijent.Ime} {pacijent.Prezime}");
                                        Thread.Sleep(20000); // Simulacija vremena obrade
                                         noviStatus = rand.Next(2) == 0 ? StatusLekar.SPREMAN_ZA_ODLAZAK : StatusLekar.PONOVITI_TERAPIJU;
                                        // Oznaka pacijenta kao obradjenog
                                        AžurirajStatusPacijenta(pacijent.LBO, noviStatus);
                                        if (noviStatus == StatusLekar.SPREMAN_ZA_ODLAZAK)
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} je spreman da ide kući.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} mora ponoviti terapiju.");
                                        }
                                    }
                                }

                                // Obrada pacijenata sa pregledima
                                foreach (var pacijent in pacijenti)
                                {
                                    if (pacijent.VrsteZahteva == VrsteZahteva.PREGLED)
                                    {
                                        // Obrada pacijenata sa pregledima
                                        Console.WriteLine($"[LekarSpecijalista] Obrada pacijenta {pacijent.Ime} {pacijent.Prezime}");
                                        Thread.Sleep(20000); // Simulacija vremena obrade
                                        StatusLekar noviStatus = rand.Next(2) == 0 ? StatusLekar.SPREMAN_ZA_ODLAZAK : StatusLekar.PONOVITI_PREGLED;
                                        // Oznaka pacijenta kao obradjenog
                                        AžurirajStatusPacijenta(pacijent.LBO, noviStatus);
                                        if (noviStatus == StatusLekar.SPREMAN_ZA_ODLAZAK)
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} je spreman da ide kući.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"[LekarSpecijalista] Pacijent {pacijent.Ime} {pacijent.Prezime} mora ponoviti pregled.");
                                        }
                                    }
                                }*/

                                // Poslati odgovor serveru
                                formatter.Serialize(ns, "Obrada pacijenata završena.");
                                ns.Flush();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LekarSpecijalista ERROR] {ex.Message}");
                        }
                        finally
                        {
                            client.Close();
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LekarSpecijalista ERROR] {ex.Message}");
            }

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();
        }

       /* static void AžurirajStatusPacijenta(int idPacijenta, StatusLekar noviStatus)
        {
            // Ažuriraj status pacijenta na serveru (kroz neki metod servera)
            Console.WriteLine($"[LekarSpecijalista] Ažuriran status pacijenta ID {idPacijenta} na {noviStatus}");
        }*/
    }
}
