using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Terapija
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6003;

            TcpListener listener = null;

            try
            {
                listener = new TcpListener(IPAddress.Any, port);

                // Postavljanje opcije ReuseAddress
                listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

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

                            using (var ns = client.GetStream())
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                zahtev = (Zahtev)formatter.Deserialize(ns);

                                Console.WriteLine($"[Terapija] Primljen zahtev:");
                                Console.WriteLine($"  Pacijent ID: {zahtev.IdPacijenta}");
                                Console.WriteLine($"  Jedinica ID: {zahtev.IdJedinice}");
                                Console.WriteLine($"  Status: {zahtev.StatusZahteva}");

                                // Simulacija operacije
                                int trajanjeOperacije = 20000;
                                Console.WriteLine($"[Terapija] Terapija u toku... ({trajanjeOperacije} ms)");
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
            catch (SocketException ex)
            {
                Console.WriteLine($"[Terapija ERROR] Socket greška: {ex.Message}");
                Console.WriteLine("Moguće je da port 6001 već koristi drugi proces.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Terapija ERROR] {ex.Message}");
            }
            finally
            {
                listener?.Stop();
            }

            Console.WriteLine("Pritisni ENTER za izlaz...");
            Console.ReadLine();
        }
    }
}
