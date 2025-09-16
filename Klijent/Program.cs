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
                   
                    const int serverPort = 5000;
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, serverPort);
                    IPacijentRepozitorijum pr = new PacijentRepozitorijum();
                    RegistracijaPacijenta registracija = new RegistracijaPacijenta();

                    while (true)
                    {
                        
                        Pacijent pacijent = registracija.Registracija();
                    
                        pr.IspisiPacijenta(pacijent);
                      
                        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            socket.Connect(serverEP);
                            Console.WriteLine("[Pacijent] Konektovan na server.");

                          
                            byte[] buffer;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(ms, pacijent);
                                buffer = ms.ToArray();
                            }

                          
                            socket.Send(buffer);
                            Console.WriteLine("[Pacijent] Podaci poslati serveru.");

                    
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
