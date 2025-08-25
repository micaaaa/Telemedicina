using Domen.Enumeracije;
using Domen.Klase;
using Domen.PomocneMetode;
using Domen.Repozitorijumi.JedinicaRepozitorijum;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using Domen.Repozitorijumi.RezultatRepozitorijum;
using Domen.Repozitorijumi.ZahtevRepozitorijum;
using System;
using System.Collections.Generic;
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
    static IRezultatRepozitorijum rezultatRepozitorijum = new RezultatRepozitorijum();
    static object lockObj = new object();

    static void Main(string[] args)
    {
        const int port = 5000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[Server] Čekam konekcije na portu {port}...");

        new Thread(ObradaZahtevaLoop) { IsBackground = true }.Start();
        new Thread(SlanjePacijenataLekaruLoop) { IsBackground = true }.Start();

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

                        pacijentRepozitorijum.DodajPacijenta(pacijent);
                        pacijentRepozitorijum.SacuvajUFajl(pacijent);

                        Console.WriteLine($"[Server] Primljen pacijent: {pacijent.Ime} {pacijent.Prezime}, zahtev: {pacijent.VrsteZahteva}");

                        PronadjiPogodnuJedinicu pronadji = new PronadjiPogodnuJedinicu();
                        Jedinica jedinica = pronadji.PronadjiPogodnu(pacijent.VrsteZahteva, jedinicaRepozitorijum);

                        if (jedinica == null)
                        {
                            client.Send(Encoding.UTF8.GetBytes("Nema dostupnih jedinica."));
                            return;
                        }

                        Zahtev zahtev = new Zahtev(pacijent.LBO, jedinica.IdJedinice, StatusZahteva.AKTIVAN);

                        lock (lockObj)
                        {
                            zahtevRepozitorijum.DodajZahtev(zahtev);
                        }

                        Console.WriteLine($"[Server] Zahtev dodat za jedinicu #{jedinica.IdJedinice}");

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
                    Console.WriteLine($"[Server] Obrađujem zahtev za pacijenta {zahtev.IdPacijenta}");

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

                    kreiranjeInstance.PokreniJedinice(jedinica.TipJedinice);

                    Pacijent pac = pacijentRepozitorijum.PronadjiPoLBO(zahtev.IdPacijenta);
                    pacijentRepozitorijum.AzurirajStatusPacijenta(pac);

                    Thread.Sleep(500);

                    using (TcpClient jedinicaClient = new TcpClient())
                    {
                        jedinicaClient.Connect("127.0.0.1", port);

                        using (NetworkStream ns = jedinicaClient.GetStream())
                        {
                            BinaryFormatter bf = new BinaryFormatter();

                            bf.Serialize(ns, zahtev);
                            ns.Flush();

                            RezultatLekar rezultatLekar = (RezultatLekar)bf.Deserialize(ns);

                            Pacijent p = pacijentRepozitorijum.PronadjiPoLBO(rezultatLekar.IdPacijenta);
                            pacijentRepozitorijum.AzurirajStatusPacijenta(p);
                            pacijentRepozitorijum.DodajObradjenogPacijenta(p);

                            rezultatRepozitorijum.dodajRezultat(rezultatLekar);

                            zahtevRepozitorijum.UkloniZavrsenZahtev(zahtev);

                            Console.WriteLine($"[Server] Obrada završena: {rezultatLekar.IdPacijenta}, {rezultatLekar.OpisRezultata}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server ERROR] Obrada: {ex.Message}");
                }
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    static void SlanjePacijenataLekaruLoop()
    {
        while (true)
        {
            try
            {
                List<Pacijent> pacijenti = pacijentRepozitorijum.VratiSveObradjene();
                List<RezultatLekar> rezultati = rezultatRepozitorijum.vratiSve();

                if (pacijenti.Count == 0)
                {
                    Thread.Sleep(10000);
                    continue;
                }

                using (TcpClient client = new TcpClient())
                {
                    client.Connect("127.0.0.1", 6004);

                    using (NetworkStream ns = client.GetStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        var paket = Tuple.Create(pacijenti, rezultati);

                        bf.Serialize(ns, paket);
                        ns.Flush();

                        // ✔ Ovde sada očekujemo Tuple sa ažuriranim pacijentima i obrađenim rezultatima
                        Tuple<List<Pacijent>, List<RezultatLekar>> odgovor =
                            (Tuple<List<Pacijent>, List<RezultatLekar>>)bf.Deserialize(ns);

                        List<Pacijent> azuriraniPacijenti = odgovor.Item1;
                        List<RezultatLekar> obradjeniRezultati = odgovor.Item2;

                        Console.WriteLine($"[Server] Primljeno {azuriraniPacijenti.Count} ažuriranih pacijenata od lekara.");

                        KreirajNoveZahteveZaAzuriranePacijente(azuriraniPacijenti);

                        // ✔✔✔ Dodato: briši sve rezultate koje je lekar obradio
                        foreach (var rez in obradjeniRezultati)
                        {
                            rezultatRepozitorijum.ukloniRezultat(rez);
                            Console.WriteLine($"[Server] Obrisan rezultat za pacijenta {rez.IdPacijenta}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server ERROR] Slanje lekaru: {ex.Message}");
            }

            Thread.Sleep(20000);
        }
    }
    private static void KreirajNoveZahteveZaAzuriranePacijente(List<Pacijent> azuriraniPacijenti)
    {
        lock (lockObj)
        {
            foreach (var pacijent in azuriraniPacijenti)
            {
                if (pacijent.Status == Status.CEKANJE_PREGLEDA ||
                    pacijent.Status == Status.CEKANJE_OPERACIJE ||
                    pacijent.Status == Status.CEKANJE_TERAPIJE)
                {
                    PronadjiPogodnuJedinicu pronadji = new PronadjiPogodnuJedinicu();
                    Jedinica jedinica = pronadji.PronadjiPogodnu(pacijent.VrsteZahteva, jedinicaRepozitorijum);

                    if (jedinica != null)
                    {
                        Zahtev noviZahtev = new Zahtev(pacijent.LBO, jedinica.IdJedinice, StatusZahteva.AKTIVAN);
                        zahtevRepozitorijum.DodajZahtev(noviZahtev);
                        Console.WriteLine($"[Server] Novi zahtev za pacijenta {pacijent.LBO} u jedinici {jedinica.IdJedinice}");
                    }
                    else
                    {
                        Console.WriteLine($"[Server] Nema jedinice za novi zahtev pacijenta {pacijent.LBO}");
                    }
                }
            }

            foreach (var pacijentAzuriran in azuriraniPacijenti)
            {
                if (pacijentAzuriran.Status == Status.CEKANJE_PREGLEDA ||
                    pacijentAzuriran.Status == Status.CEKANJE_OPERACIJE ||
                    pacijentAzuriran.Status == Status.CEKANJE_TERAPIJE ||
                    pacijentAzuriran.Status == Status.OBAVLJENA_OPERACIJA ||
                    pacijentAzuriran.Status == Status.OBAVLJEN_PREGLED ||
                    pacijentAzuriran.Status == Status.OBAVLJENA_TERAPIJA)
                {
                    var zaBrisanje = pacijentRepozitorijum.VratiSveObradjene()
                        .Find(p => p.LBO == pacijentAzuriran.LBO);
                    if (zaBrisanje != null)
                    {
                        pacijentRepozitorijum.UkloniObradjenog(zaBrisanje);
                        Console.WriteLine($"[Server] Uklonjen pacijent {zaBrisanje.LBO} iz obradjenih.");
                    }
                }
            }
        }
    }
}


