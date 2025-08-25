using Domen.Enumeracije;
using Domen.Klase;
using Domen.PomocneMetode;
using Domen.Repozitorijumi.JedinicaRepozitorijum;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using Domen.Repozitorijumi.ZahtevRepozitorijum;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

class Server
{
    static IPacijentRepozitorijum pacijentRepozitorijum = new PacijentRepozitorijum();
    static JedinicaRepozitorijum jedinicaRepozitorijum = new JedinicaRepozitorijum();
    static IZahtevRepozitorijum zahtevRepozitorijum = new ZahtevRepozitorijum(pacijentRepozitorijum);
    static IspisPacijenta ispisPacijenta = new IspisPacijenta();
    static object lockObj = new object();

    static void Main(string[] args)
    {
        const int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Čekam konekcije na portu {port}...");

        // Pokrećemo thread koji obrađuje zahteve iz reda
        new Thread(ObradaZahtevaLoop) { IsBackground = true }.Start();

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
                        Pacijent pacijent = (Pacijent)formatter.Deserialize(ms);

                        // Dodaj pacijenta u repozitorijum (ako želiš)
                        pacijentRepozitorijum.DodajPacijenta(pacijent);
                        pacijentRepozitorijum.SacuvajUFajl(pacijent);

                        Console.WriteLine($"[Server] Primljen pacijent: {pacijent.Ime} {pacijent.Prezime}, zahtev: {pacijent.VrsteZahteva}");

                        // Pronađi odgovarajuću jedinicu za tip zahteva
                        PronadjiPogodnuJedinicu pronadji = new PronadjiPogodnuJedinicu();
                        Jedinica jedinica = pronadji.PronadjiPogodnu(pacijent.VrsteZahteva, jedinicaRepozitorijum);

                        if (jedinica == null)
                        {
                            Console.WriteLine("[Server] Nema dostupne jedinice za ovaj zahtev.");
                            client.Send(Encoding.UTF8.GetBytes("Nema dostupnih jedinica."));
                            return;
                        }

                        // Kreiraj zahtev za obradu
                        Zahtev zahtev = new Zahtev(pacijent.LBO, jedinica.IdJedinice, StatusZahteva.AKTIVAN);

                        lock (lockObj)
                        {
                            zahtevRepozitorijum.DodajZahtev(zahtev);
                        }

                        Console.WriteLine($"[Server] Zahtev dodat u red za jedinicu #{jedinica.IdJedinice} (Tip: {jedinica.TipJedinice})");

                        // Potvrdi klijentu da je zahtev prihvaćen i stavi ga u red
                        client.Send(Encoding.UTF8.GetBytes("Zahtev prihvaćen i stavljen u red za obradu."));
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

    static void ObradaZahtevaLoop()
    {
        KreiranjeInstanceJedinice kreiranjeInstance = new KreiranjeInstanceJedinice();

        while (true)
        {
            Zahtev zahtev = null;

            lock (lockObj)
            {
                zahtev = zahtevRepozitorijum.UzmiSledeciZahtevZaObradu();
            }
            

            if (zahtev != null)
            {
                try
                {
                    Console.WriteLine($"[Server] Obrađujem zahtev za pacijenta {zahtev.IdPacijenta} u jedinici {zahtev.IdJedinice}");

                    // Pronađi jedinicu po IdJedinice da bi znao tip i port
                    Jedinica jedinica = jedinicaRepozitorijum.GetById(zahtev.IdJedinice);
                    if (jedinica == null)
                    {
                        Console.WriteLine("[Server] Greška: Jedinica nije pronađena!");
                        continue;
                    }

                    int port = 0;

                    switch (jedinica.TipJedinice)
                    {
                        case TipJedinice.URGENTNA:
                            port = 6001;
                            break;
                        case TipJedinice.DIJAGNOSTICKA:
                            port = 6002;
                            break;
                        case TipJedinice.TERAPEUTSKA:
                            port = 6003;
                            break;
                        default:
                            Console.WriteLine("[Server] Nepoznat tip jedinice!");
                            continue;
                    }

                    // Pokreni jedinicu pre konekcije
                    kreiranjeInstance.PokreniJedinice(jedinica.TipJedinice);
                    Pacijent pac = pacijentRepozitorijum.PronadjiPoLBO(zahtev.IdPacijenta);
                    pacijentRepozitorijum.AzurirajStatusPacijenta(pac);
                    // Sačekaj da se jedinica pokrene i otvori port
                    Thread.Sleep(500);

                    using (TcpClient jedinicaClient = new TcpClient())
                    {
                        jedinicaClient.Connect("127.0.0.1", port);

                        using (NetworkStream ns = jedinicaClient.GetStream())
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            
                            // Pošaljemo zahtev jedinici
                            bf.Serialize(ns, zahtev);
                            ns.Flush();
                       
                            // Sačekaj odgovor
                            Console.WriteLine($"Pacijent {pac.LBO} i status {pac.Status}");
                            RezultatLekar rezultatLekar = (RezultatLekar)bf.Deserialize(ns);
                            Pacijent p = pacijentRepozitorijum.PronadjiPoLBO(rezultatLekar.IdPacijenta);
                            pacijentRepozitorijum.AzurirajStatusPacijenta(p);
                            Console.WriteLine($"Pacijent {pac.LBO} i status {pac.Status}");
                            Console.WriteLine($"[Server] Jedinica završila obradu: Pacijent ID: {rezultatLekar.IdPacijenta}");
                            Console.WriteLine($"[UrgentnaJedinica] Poslat rezultat lekara:");
                            Console.WriteLine($"  Pacijent ID: {rezultatLekar.IdPacijenta}");
                            Console.WriteLine($"  Vreme: {rezultatLekar.Vreme}");
                            Console.WriteLine($"  Rezultat: {rezultatLekar.OpisRezultata}");


                            // Ukloni zahtev nakon završene obrade
                            zahtevRepozitorijum.UkloniZavrsenZahtev(zahtev);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server ERROR] Greška pri obradi zahteva: {ex.Message}");
                }
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }
}
