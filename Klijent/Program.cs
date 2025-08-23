// Klijent/Program.cs
using Domen.Klase;
using Domen.PomocneMetode;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Klijent
{
    public class Program
    {
        static void Main(string[] args)
        {
            new Thread(() =>
            {
                try
                {
                    const string serverIp = "127.0.0.1";
                    const int serverPort = 5000;

                    IPacijentRepozitorijum pr = new PacijentRepozitorijum();
                    RegistracijaPacijenta registracija = new RegistracijaPacijenta();

                    while (true)
                    {
                        Console.WriteLine("Unesite podatke za novog pacijenta:");
                        Pacijent pacijent = registracija.Registracija();
                        pr.DodajPacijenta(pacijent);

                        // Kreiraj i poveži socket za svaki pacijent posebno
                        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            socket.Connect(IPAddress.Parse(serverIp), serverPort);
                            Console.WriteLine("[Pacijent] Konektovan na server.");

                            // Serijalizuj samo objekat pacijenta
                            byte[] buffer;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(ms, pacijent);
                                buffer = ms.ToArray();
                            }

                            // Pošalji pacijenta serveru
                            socket.Send(buffer);
                            Console.WriteLine("[Pacijent] Podaci poslati serveru.");

                            // Primi odgovor
                            byte[] ack = new byte[1024];
                            int recv = socket.Receive(ack);
                            string odgovor = Encoding.UTF8.GetString(ack, 0, recv);
                            Console.WriteLine($"[Pacijent] Server javlja: {odgovor}");

                            socket.Close();
                        }

 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Pacijent ERROR] {ex.Message}");
                }
            })
            { IsBackground = false }.Start();
        }
    }
}
