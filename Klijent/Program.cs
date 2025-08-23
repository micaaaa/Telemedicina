using Domen.Enumeracije;
using Domen.Klase;
using Domen.PomocneMetode;
using Domen.Repozitorijumi.JedinicaRepozitorijum;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using static Domen.Klase.ZahtevPacijent;

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

                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(IPAddress.Parse(serverIp), serverPort);
                    Console.WriteLine("[Pacijent] Konektovan na server.");

                    IPacijentRepozitorijum pr = new PacijentRepozitorijum();

                    // Napravi pacijenta
                    RegistracijaPacijenta r = new RegistracijaPacijenta();
                   // KreirajZahtev kz = new KreirajZahtev();
                    Pacijent p = r.Registracija();
                    pr.DodajPacijenta(p);
                    pr.SacuvajUFajl(p);
                    JedinicaRepozitorijum repozitorijum = new JedinicaRepozitorijum();
                    PronadjiPogodnuJedinicu pronadji = new PronadjiPogodnuJedinicu();

                    // Pronalazak odgovarajuće jedinice na osnovu vrste zahteva pacijenta
                    Jedinica pogodnaJedinica = pronadji.PronadjiPogodnu(p.VrsteZahteva, repozitorijum);

                    Zahtev noviZahtev = null;


                    if (pogodnaJedinica != null)
                    {
                        // Kreiranje zahteva sa idPacijenta i idJedinice
                        noviZahtev = new Zahtev(p.LBO, pogodnaJedinica.IdJedinice, StatusZahteva.AKTIVAN);

                        Console.WriteLine($"Zahtev za pacijenta {p.Ime} {p.Prezime} je kreiran.");
                        Console.WriteLine($"Dodeljena jedinica ID: {pogodnaJedinica.IdJedinice}, Tip: {pogodnaJedinica.TipJedinice}");
                    }
                    else
                    {
                        Console.WriteLine("Nema dostupnih jedinica za dati zahtev.");
                    }

                    // Kreiraj DTO objekat koji sadrži i pacijenta i zahtev
                    ZahtevPacijentDTO dto = new ZahtevPacijentDTO(p, noviZahtev);




                    // Serijalizuj DTO objekat
                    byte[] buffer;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, dto);
                        buffer = ms.ToArray();
                    }

                    // Pošalji DTO serveru
                    socket.Send(buffer);
                    Console.WriteLine("[Pacijent] Podaci poslati serveru.");

                    // Primi odgovor
                    byte[] ack = new byte[1024];
                    int recv = socket.Receive(ack);
                    string odgovor = Encoding.UTF8.GetString(ack, 0, recv);
                    Console.WriteLine($"[Pacijent] Server kaže: {odgovor}");

                    socket.Close();
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
