using Domen.Enumeracije;
using Domen.Klase;
using Domen.Repozitorijumi.JedinicaRepozitorijum;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using Domen.Repozitorijumi.RezultatRepozitorijum;
using Domen.Repozitorijumi.ZahtevRepozitorijum;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Dijagnostika
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6002;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            IPacijentRepozitorijum pacijentRepozitorijum = new PacijentRepozitorijum();
            IZahtevRepozitorijum zahtevRepozitorijum = new ZahtevRepozitorijum(pacijentRepozitorijum);
            IRezultatRepozitorijum rezultatRepozitorijum = new RezultatRepozitorijum();

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("[Dijagnostika] Jedinica sluša na portu 6002...");

                while (true)
                {
                    Socket clientSocket = listener.Accept();
                    Console.WriteLine("[Dijagnostika] Primljena konekcija.");

                    new Thread(() =>
                    {
                        try
                        {
                            byte[] buffer = new byte[8192];
                            int received = clientSocket.Receive(buffer);

                            Zahtev zahtev;
                            Pacijent pacijent;

                            using (MemoryStream msReceive = new MemoryStream(buffer, 0, received))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                var tuple = (Tuple<Zahtev, Pacijent>)formatter.Deserialize(msReceive);
                                zahtev = tuple.Item1;
                                pacijent = tuple.Item2;
                            }

                            zahtevRepozitorijum.IspisiZahtev(zahtev);
                            pacijentRepozitorijum.IspisiPacijenta(pacijent);

                            int trajanjeDijagnoze = 20000;
                            Console.WriteLine($"[Dijagnostika] Dijagnostika u toku... ({trajanjeDijagnoze} ms)");
                            zahtev.StatusZahteva = StatusZahteva.U_OBRADI;
                            Thread.Sleep(trajanjeDijagnoze);
                            zahtev.StatusZahteva = StatusZahteva.ZAVRSEN;

                            // Ručni unos rezultata
                            OpisRezultata opis;
                            while (true)
                            {
                                Console.WriteLine("Unesite ishod dijagnoze:");
                                Console.WriteLine("1 - Dijagnoza ustanovljena");
                                Console.WriteLine("2 - Dijagnoza nije ustanovljena");
                                string unos = Console.ReadLine();

                                if (unos == "1")
                                {
                                    opis = OpisRezultata.DIJAGNOZA_USTANOVLJENA;
                                    break;
                                }
                                else if (unos == "2")
                                {
                                    opis = OpisRezultata.DIJAGNOZA_NIJE_USTANOVLJENA;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Nevažeći unos. Pokušajte ponovo.");
                                }
                            }

                            RezultatLekar rezultat = new RezultatLekar(zahtev.IdPacijenta, DateTime.Now, opis);

                            Console.WriteLine("[Dijagnostika] Poslat rezultat:");
                            rezultatRepozitorijum.IspisiRezultat(rezultat);

                            using (MemoryStream msSend = new MemoryStream())
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                formatter.Serialize(msSend, rezultat);
                                byte[] rezultatBytes = msSend.ToArray();

                                clientSocket.Send(rezultatBytes);
                                clientSocket.Shutdown(SocketShutdown.Send);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Dijagnostika ERROR] {ex.Message}");
                        }
                        finally
                        {
                            if (clientSocket.Connected)
                                clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
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
