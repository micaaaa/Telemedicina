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
        public static void Main(string[] args)
        {
            int port = 6004;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(5);

            Console.WriteLine("[LekarSpecijalista] Lekar sluša na portu 6004...");

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("[LekarSpecijalista] Server povezan.");

                new Thread(() =>
                {
                    try
                    {
                        
                        byte[] buffer = new byte[8192];
                        int received = 0;

                       
                        received = clientSocket.Receive(buffer);

                        if (received == 0)
                        {
                            Console.WriteLine("[LekarSpecijalista] Nema primljenih podataka.");
                            clientSocket.Close();
                            return;
                        }

                        Tuple<List<Pacijent>, List<RezultatLekar>> paket;
                        using (MemoryStream ms = new MemoryStream(buffer, 0, received))
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            paket = (Tuple<List<Pacijent>, List<RezultatLekar>>)formatter.Deserialize(ms);
                        }

                        List<Pacijent> pacijenti = paket.Item1;
                        List<RezultatLekar> rezultati = paket.Item2;
                        
                   
                        
                        Console.WriteLine($"[LekarSpecijalista] Primljeno {pacijenti.Count} pacijenata i {rezultati.Count} rezultata.");
                        Console.WriteLine();
                        Console.WriteLine("============================= PACIJENTI =============================");
                        Console.WriteLine("|   LBO   |    Ime    |   Prezime   |     Vrsta zahteva    |   Status   |");
                        Console.WriteLine("|---------|-----------|-------------|-----------------------|------------|");

                        foreach (var p in pacijenti)
                        {
                            Console.WriteLine($"| {p.LBO,-7} | {p.Ime,-9} | {p.Prezime,-11} | {p.VrsteZahteva,-21} | {p.Status,-10} |");
                        }

                        Console.WriteLine("=====================================================================\n");
                        Console.WriteLine("=========================== REZULTATI LEKARA ============================");
                        Console.WriteLine("| ID Pacijenta |     Vreme      |       Opis rezultata       |");
                        Console.WriteLine("|--------------|----------------|-----------------------------|");

                        foreach (var r in rezultati)
                        {
                            Console.WriteLine($"| {r.IdPacijenta,-13} | {r.Vreme:yyyy-MM-dd HH:mm} | {r.OpisRezultata,-27} |");
                        }

                        Console.WriteLine("=========================================================================\n");


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

                        byte[] odgovorBuffer;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            var odgovor = Tuple.Create(pacijenti, rezultati);
                            formatter.Serialize(ms, odgovor);
                            odgovorBuffer = ms.ToArray();
                        }
                       
                        clientSocket.Send(odgovorBuffer);

                        Console.WriteLine("[LekarSpecijalista] Odgovor poslat serveru.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LekarSpecijalista ERROR] {ex.Message}");
                    }
                    finally
                    {
                        clientSocket.Close();
                    }
                }).Start();
            }
        }

        public static void ObradiPacijente(List<Pacijent> lista, List<RezultatLekar> rezultati)
        {
            foreach (var pacijent in lista)
            {
                Thread.Sleep(500);

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
