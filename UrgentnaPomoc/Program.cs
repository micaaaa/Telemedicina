using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Domen.Klase;
using Domen.Enumeracije;

namespace UrgentnaPomoc
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
                Console.WriteLine("[UrgentnaJedinica] Jedinica sluša na portu 6001...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("[UrgentnaJedinica] Primljena konekcija.");

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

                                Console.WriteLine($"[UrgentnaJedinica] Primljen zahtev:");
                                Console.WriteLine($"  Pacijent ID: {zahtev.IdPacijenta}");
                                Console.WriteLine($"  Jedinica ID: {zahtev.IdJedinice}");
                                Console.WriteLine($"  Status: {zahtev.StatusZahteva}");


                                int trajanjeOperacije = 20000;
                                Console.WriteLine($"[UrgentnaJedinica] Operacija u toku... ({trajanjeOperacije} ms)");
                                zahtev.StatusZahteva = StatusZahteva.U_OBRADI;
                                Thread.Sleep(trajanjeOperacije);

                                // Ažuriraj status zahteva
                                zahtev.StatusZahteva = StatusZahteva.ZAVRSEN;

                                // Pošalji nazad ceo objekat zahtev serveru
                                formatter.Serialize(ns, zahtev);
                                ns.Flush();

                                Console.WriteLine("[UrgentnaJedinica] Poslat ažurirani zahtev serveru.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[UrgentnaJedinica ERROR] {ex.Message}");
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
                Console.WriteLine($"[UrgentnaJedinica ERROR] {ex.Message}");
            }

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();
        }
    }
}
