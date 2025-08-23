using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Klijent
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dobro dosli!");

            const string serverIp = "127.0.0.1";
            const int serverPort = 5000;

            new Thread(() =>
            {
                try
                {
                    //  Kreiramo TCP socket i povežemo se
                    Socket sock = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                    sock.Connect(new IPEndPoint(IPAddress.Parse(serverIp), serverPort));

                    //kreiramo drugi tcp socket za obavjestenje o isporucenoj porudzbini
                    Socket orderSock = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);
                    orderSock.Connect(new IPEndPoint(IPAddress.Parse(serverIp), 4011));

                    // Pošaljemo REGISTER;{waiterId};Waiter;{udpPort}\n
                    string regMsg = $"REGISTROVAN;{1};Pacijent;{5001}\n";
                    sock.Send(Encoding.UTF8.GetBytes(regMsg));

                    // Prihvatimo ACK liniju “REGISTERED\n”
                    var tmp = new byte[1];


                    byte[] ackbytes = new byte[1024];
                    int bytesRecieved = sock.Receive(ackbytes);
                    string ack = Encoding.UTF8.GetString(ackbytes, 0, bytesRecieved).Trim();
                    if (ack != "REGISTERED")
                    {
                        Console.WriteLine($"\nREGISTRACIJA NEUSPESNA");
                    }
                    else
                    {
                        Console.WriteLine("\nUSPESNO REGISTROVAN");
                    }

                    // Beskonacna petlja za READY;{tableNo};{waiterId}\n

                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int r = sock.Receive(buffer);

                        string line = Encoding.UTF8.GetString(buffer).Trim();

                        if (line.StartsWith("POCETAK;"))
                        {
                            var parts = line.Split(';');
                            int tableNo = int.Parse(parts[1]);
                            string tipPorudzbine = parts[2];
                            Console.WriteLine($"Porudžbina za sto {tableNo} je spremna! Nosim…");
                            Thread.Sleep(1500);
                            Console.WriteLine($"Porudžbina {tipPorudzbine} za sto {tableNo} je dostavljena.");
                            orderSock.Send(Encoding.UTF8.GetBytes($"DELIVERED;{tableNo};{tipPorudzbine}"));

                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotificationThread ERROR] {ex.Message}");
                }
            })
            { IsBackground = true }
  .Start();


        }
    }
}
