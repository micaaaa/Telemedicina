using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Domen.Klase;
using Domen.Enumeracije;

namespace LekarSpecijalista
{
    public class Program
    {
        static void Main(string[] args)
        {
            int port = 6004;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("[LekarSpecijalista] Lekar sluša na portu 6004...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("[LekarSpecijalista] Server povezan.");

                new Thread(() =>
                {
                    try
                    {
                        using (NetworkStream ns = client.GetStream())
                        {
                            BinaryFormatter formatter = new BinaryFormatter();

                            var paket = (Tuple<List<Pacijent>, List<RezultatLekar>>)formatter.Deserialize(ns);
                            List<Pacijent> pacijenti = paket.Item1;
                            List<RezultatLekar> rezultati = paket.Item2;

                            Console.WriteLine($"[LekarSpecijalista] Primljeno {pacijenti.Count} pacijenata i {rezultati.Count} rezultata.");

                            List<Pacijent> urgenti = new List<Pacijent>();
                            List<Pacijent> ostali = new List<Pacijent>();

                            foreach (var pacijent in pacijenti)
                            {
                                if (pacijent.VrsteZahteva == VrsteZahteva.URGENTA_POMOC)
                                    urgenti.Add(pacijent);
                                else
                                    ostali.Add(pacijent);
                            }

                            ObradiPacijente(urgenti, rezultati);
                            ObradiPacijente(ostali, rezultati);

                            // ✔✔✔ Dodato: šaljemo nazad Tuple sa pacijentima i obradjenim rezultatima
                            var odgovor = Tuple.Create(pacijenti, rezultati);
                            formatter.Serialize(ns, odgovor);
                            ns.Flush();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LekarSpecijalista ERROR] {ex.Message}");
                    }
                    finally
                    {
                        client.Close();
                    }
                }).Start();
            }
        }

        public static void ObradiPacijente(List<Pacijent> lista, List<RezultatLekar> rezultati)
        {
            foreach (var pacijent in lista)
            {
                Thread.Sleep(500); // simulacija obrade

                RezultatLekar rezultat = rezultati.Find(r => r.IdPacijenta == pacijent.LBO);

                if (rezultat != null)
                {
                    switch (pacijent.VrsteZahteva)
                    {
                        case VrsteZahteva.URGENTA_POMOC:
                            if (rezultat.OpisRezultata == OpisRezultata.OPERACIJA_NEUSPESNA)
                                pacijent.Status = Status.CEKANJE_OPERACIJE;
                            break;

                        case VrsteZahteva.TERAPIJA:
                            if (rezultat.OpisRezultata == OpisRezultata.TERAPIJA_NEUSPESNA)
                                pacijent.Status = Status.CEKANJE_PREGLEDA;
                            break;

                        case VrsteZahteva.PREGLED:
                            if (rezultat.OpisRezultata == OpisRezultata.DIJAGNOZA_NIJE_USTANOVLJENA)
                                pacijent.Status = Status.CEKANJE_PREGLEDA;
                            break;
                    }
                }

                Console.WriteLine($"  - Lekar obradio pacijenta: {pacijent.Ime} {pacijent.Prezime} -> {pacijent.Status}");
            }
        }
    }
}
