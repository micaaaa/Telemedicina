using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Dijagnostika
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6002;

            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine("[Dijagnostika] Jedinica sluša na portu 6002...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("[Dijagnostika] Primljena konekcija.");

                    new Thread(() =>
                    {
                        try
                        {
                            Zahtev zahtev;

                            using (NetworkStream ns = client.GetStream())
                            {
                                // Direktno deserijalizuj sa NetworkStream (bez ručnog čitanja bajtova)
                                BinaryFormatter formatter = new BinaryFormatter();
                                zahtev = (Zahtev)formatter.Deserialize(ns);

                                Console.WriteLine($"[Dijagnostika] Primljen zahtev:");
                                Console.WriteLine($"  Pacijent ID: {zahtev.IdPacijenta}");
                                Console.WriteLine($"  Jedinica ID: {zahtev.IdJedinice}");
                                Console.WriteLine($"  Status: {zahtev.StatusZahteva}");

                                // Simulacija dijagnoze
                                int trajanjeDijagnoze = 20000;
                                Console.WriteLine($"[Dijagnostika] Dijagnostika u toku... ({trajanjeDijagnoze} ms)");
                                zahtev.StatusZahteva = StatusZahteva.U_OBRADI;
                                Thread.Sleep(trajanjeDijagnoze);

                                // Ažuriraj status zahteva
                                zahtev.StatusZahteva = StatusZahteva.ZAVRSEN;

                                // Kreiraj RezultatLekar objekat
                                DateTime vreme = DateTime.Now;
                                Random rand = new Random();
                                OpisRezultata opis = rand.Next(2) == 0 ? OpisRezultata.DIJAGNOZA_USTANOVLJENA : OpisRezultata.DIJAGNOZA_NIJE_USTANOVLJENA;

                                RezultatLekar rl = new RezultatLekar(zahtev.IdPacijenta, vreme, opis);

                                Console.WriteLine("[Dijagnostika] Poslat rezultat lekara:");
                                Console.WriteLine($"  Pacijent ID: {rl.IdPacijenta}");
                                Console.WriteLine($"  Vreme: {rl.Vreme}");
                                Console.WriteLine($"  Rezultat: {rl.OpisRezultata}");

                                // Pošaljemo RezultatLekar nazad serveru umesto Zahteva
                                formatter.Serialize(ns, rl);
                                ns.Flush();

                                Console.WriteLine("[Dijagnostika] Poslat rezultat lekara serveru.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Dijagnostika ERROR] {ex.Message}");
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
                Console.WriteLine($"[Dijagnostika ERROR] {ex.Message}");
            }

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();
        }
    }
}
