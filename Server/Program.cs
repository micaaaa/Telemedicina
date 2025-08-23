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

class Server
{
    static void Main(string[] args)
    {
        const int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Čekam konekcije na portu {port}...");

        IPacijentRepozitorijum pr = new PacijentRepozitorijum();
        IspisPacijenta ispisPacijenta = new IspisPacijenta();
        pr.UcitajIzFajla();

        while (true)
        {
            Socket client = listener.AcceptSocket();
            Console.WriteLine("[Server] Povezan pacijent!");

            new Thread(() =>
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    int received = client.Receive(buffer);

                    using (MemoryStream ms = new MemoryStream(buffer, 0, received))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        ZahtevPacijentDTO dto = (ZahtevPacijentDTO)formatter.Deserialize(ms);

                        // Dodaj pacijenta u centralnu listu i sačuvaj
                        pr.DodajPacijenta(dto.Pacijent);
                        pr.SacuvajUFajl(dto.Pacijent);

                        Zahtev z = dto.Zahtev;

                        Console.WriteLine($"[Server] Primljen zahtev: LBO: {z.IdPacijenta}, Jedinica: {z.IdJedinice}, Status: {z.StatusZahteva}");
                        client.Send(Encoding.UTF8.GetBytes("PRIHVACENO"));

                        JedinicaRepozitorijum j = new JedinicaRepozitorijum();
                        var jedinica = j.PronadjiJedinicu(z.IdJedinice);
                        if (jedinica == null)
                        {
                            Console.WriteLine($"[Greška] Jedinica sa ID {z.IdJedinice} ne postoji.");
                            return;
                        }

                        Console.WriteLine($"[Server] Zahtev prosleđen jedinici #{jedinica.IdJedinice} (TIP: {jedinica.TipJedinice})");

                        KreiranjeInstanceJedinice kr = new KreiranjeInstanceJedinice();
                        kr.PokreniJedinice(jedinica.TipJedinice);

                        int portJedinice = 6001;
                        string ipJedinice = "127.0.0.1";

                        using (TcpClient jedinicaClient = new TcpClient())
                        {
                            jedinicaClient.Connect(ipJedinice, portJedinice);
                            using (NetworkStream ns = jedinicaClient.GetStream())
                            {
                                BinaryFormatter bf = new BinaryFormatter();

                                // Pošalji zahtev jedinici direktno
                                bf.Serialize(ns, z);
                                ns.Flush();

                                // Primi odgovor
                                Zahtev odgovorZahtev = (Zahtev)bf.Deserialize(ns);
                                Console.WriteLine($"[Server] Odgovor od jedinice: LBO: {odgovorZahtev.IdPacijenta}, Jedinica: {odgovorZahtev.IdJedinice}, Status: {odgovorZahtev.StatusZahteva}");

                                Pacijent pac = pr.PronadjiPoLBO(odgovorZahtev.IdPacijenta);
                                ispisPacijenta.ispisiPacijenta(pac);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server ERROR] {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
            }).Start();
        }
    }
}
