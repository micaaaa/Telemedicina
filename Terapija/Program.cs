using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Terapija
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6001;

            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine("[Terapija] Jedinica sluša na portu 6001...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("[Terapija] Primljena konekcija.");

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

                                Console.WriteLine($"[Terapija] Primljen zahtev:");
                                Console.WriteLine($"  Pacijent ID: {zahtev.IdPacijenta}");
                                Console.WriteLine($"  Jedinica ID: {zahtev.IdJedinice}");
                                Console.WriteLine($"  Status: {zahtev.StatusZahteva}");

                                // Simulacija operacije
                                Random rnd = new Random();
                                int trajanjeOperacije = rnd.Next(2000, 5000);
                                Console.WriteLine($"[Terapija] Trapija u toku... ({trajanjeOperacije} ms)");
                                zahtev.StatusZahteva = StatusZahteva.U_OBRADI;
                                Thread.Sleep(trajanjeOperacije);

                                // Ažuriraj status zahteva
                                zahtev.StatusZahteva = StatusZahteva.ZAVRSEN;

                                // Pošalji nazad ceo objekat zahtev serveru
                                formatter.Serialize(ns, zahtev);
                                ns.Flush();

                                Console.WriteLine("[Terapija] Poslat ažurirani zahtev serveru.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Terapija ERROR] {ex.Message}");
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
                Console.WriteLine($"[Terapija ERROR] {ex.Message}");
            }

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();
        }
    }
}
