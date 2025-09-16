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
using System.Threading.Tasks;

class Server
{
    static IPacijentRepozitorijum pacijentRepozitorijum = new PacijentRepozitorijum();
    static IJedinicaRepozitorijum jedinicaRepozitorijum = new JedinicaRepozitorijum();
    static IZahtevRepozitorijum zahtevRepozitorijum = new ZahtevRepozitorijum(pacijentRepozitorijum);
    static IRezultatRepozitorijum rezultatRepozitorijum = new RezultatRepozitorijum();
    static object lockObj = new object();

    static Socket serverSocket;
    static List<Socket> clients = new List<Socket>();
    static object clientsLock = new object();

    static void Main(string[] args)
    {
        const int port = 5000;

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
        serverSocket.Listen(5);
        serverSocket.Blocking = false; // neblokirajući socket

        Console.WriteLine($"[Server] Čekam konekcije na portu {port}...");

        // Pokrećemo obradu zahteva u posebnoj niti
        new Thread(ObradaZahtevaLoop) { IsBackground = true }.Start();
        // Pokrećemo slanje podataka lekaru u posebnoj niti
        new Thread(SlanjePacijenataLekaruLoop) { IsBackground = true }.Start();

        while (true)
        {
            try
            {
                // Pravljenje liste soketa za praćenje sa Select-om
                List<Socket> checkRead = new List<Socket>();
                checkRead.Add(serverSocket);

                lock (clientsLock)
                {
                    checkRead.AddRange(clients);
                }

                // Select sa timeoutom od 1 sekunde (1,000,000 mikrosekundi)
                Socket.Select(checkRead, null, null, 1000000);

                foreach (Socket socket in checkRead)
                {
                    if (socket == serverSocket)
                    {
                        // Nova konekcija
                        try
                        {
                            Socket client = serverSocket.Accept();
                            client.Blocking = false; // neblokirajući klijent

                            lock (clientsLock)
                            {
                                clients.Add(client);
                            }

                            Console.WriteLine("[Server] Povezan pacijent (multiplexing)!");
                        }
                        catch (SocketException)
                        {
                            // Nema nove konekcije trenutno, ignoriši
                        }
                    }
                    else
                    {
                        // Podaci od klijenta
                        try
                        {
                            byte[] buffer = new byte[4096];
                            int received = socket.Receive(buffer);

                            if (received == 0)
                            {
                                // Klijent je zatvorio konekciju
                                Console.WriteLine("[Server] Klijent zatvorio konekciju.");
                                lock (clientsLock)
                                {
                                    clients.Remove(socket);
                                }
                                socket.Close();
                                continue;
                            }

                            using (MemoryStream ms = new MemoryStream(buffer, 0, received))
                            {
                                BinaryFormatter formatter = new BinaryFormatter();
                                Pacijent pacijent = (Pacijent)formatter.Deserialize(ms);

                                Console.WriteLine($"[Server] Primljen pacijent.");
                                pacijentRepozitorijum.IspisiPacijenta(pacijent);
                                pacijentRepozitorijum.DodajPacijenta(pacijent);
                                pacijentRepozitorijum.SacuvajUFajl();

                                PronadjiPogodnuJedinicu pronadji = new PronadjiPogodnuJedinicu();
                                Jedinica jedinica = pronadji.PronadjiPogodnu(pacijent.VrsteZahteva, jedinicaRepozitorijum);

                                if (jedinica == null)
                                {
                                    socket.Send(Encoding.UTF8.GetBytes("[Server] Nema dostupnih jedinica."));
                                    continue;
                                }

                              
                                jedinicaRepozitorijum.AzurirajStatus(jedinica);

                                Console.WriteLine("[Server] Pronadjena slobodna jedinica:");
                                jedinicaRepozitorijum.IspisiJedinicu(jedinica);

                                Zahtev zahtev = new Zahtev(pacijent.LBO, jedinica.IdJedinice, StatusZahteva.AKTIVAN);

                                lock (lockObj)
                                {
                                    zahtevRepozitorijum.DodajZahtev(zahtev);
                                }
                                Console.WriteLine("[Server] Kreiran zahtev");
                                zahtevRepozitorijum.IspisiZahtev(zahtev);

                                socket.Send(Encoding.UTF8.GetBytes("Zahtev prihvaćen i stavljen u red za obradu."));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Server ERROR] Greška u komunikaciji sa klijentom: {ex.Message}");
                            lock (clientsLock)
                            {
                                clients.Remove(socket);
                            }
                            socket.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Server ERROR] Glavna petlja: " + ex.Message);
            }

            Thread.Sleep(10); // da ne ide 100% CPU
        }
    }
    static void ObradaZahtevaLoop()
    {
        while (true)
        {
            Zahtev zahtev = null;

            lock (lockObj)
            {
                zahtev = zahtevRepozitorijum.UzmiSledeciZahtevZaObradu();
            }

            if (zahtev != null)
            {
                // Pokreni obradu u novoj niti / tasku - ne blokira glavnu petlju
                Task.Run(() => ObradiZahtev(zahtev));
            }
            else
            {
                Thread.Sleep(100); // nema zahteva, malo čekaj
            }
        }
    }

    static void ObradiZahtev(Zahtev zahtev)
    {
        try
        {
            Console.WriteLine($"[Server] Obrađujem zahtev za pacijenta {zahtev.IdPacijenta}");

            Jedinica jedinica = jedinicaRepozitorijum.GetById(zahtev.IdJedinice);
            if (jedinica == null)
            {
                Console.WriteLine("[Server] Greška: Jedinica nije pronađena!");
                return;
            }

            int port;

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
                    Console.WriteLine("[Server] Nepoznat tip jedinice.");
                    return;
            }

            Pacijent pac = pacijentRepozitorijum.PronadjiPoLBO(zahtev.IdPacijenta);
            pacijentRepozitorijum.AzurirajStatusPacijenta(pac);

            using (Socket jedinicaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                jedinicaSocket.Blocking = false;
                IPEndPoint jedinicaEP = new IPEndPoint(IPAddress.Loopback, port);

                try
                {
                    jedinicaSocket.Connect(jedinicaEP);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.WouldBlock &&
                        ex.SocketErrorCode != SocketError.InProgress &&
                        ex.SocketErrorCode != SocketError.AlreadyInProgress)
                    {
                        Console.WriteLine($"[Server] Greška prilikom konekcije ka jedinici: {ex.Message}");
                        return;
                    }
                }

                List<Socket> writeList = new List<Socket> { jedinicaSocket };
                Socket.Select(null, writeList, null, 1000000);

                if (writeList.Count == 0)
                {
                    Console.WriteLine("[Server] Konekcija ka jedinici nije uspostavljena u roku.");
                    return;
                }

                var paketZaSlanje = Tuple.Create(zahtev, pac);
                byte[] bufferToSend;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, paketZaSlanje);
                    bufferToSend = ms.ToArray();
                }

                int totalSent = 0;
                while (totalSent < bufferToSend.Length)
                {
                    writeList.Clear();
                    writeList.Add(jedinicaSocket);
                    Socket.Select(null, writeList, null, 1000000);

                    if (writeList.Count > 0)
                    {
                        int sent = jedinicaSocket.Send(bufferToSend, totalSent, bufferToSend.Length - totalSent, SocketFlags.None);
                        totalSent += sent;
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }

                Console.WriteLine($"[Server] Zahtev poslat jedinici {jedinica.IdJedinice}.");

                List<byte> receivedData = new List<byte>();
                byte[] recvBuffer = new byte[4096];
                List<Socket> readList = new List<Socket>();

                while (true)
                {
                    readList.Clear();
                    readList.Add(jedinicaSocket);
                    Socket.Select(readList, null, null, 1000000);

                    if (readList.Count > 0)
                    {
                        int received = jedinicaSocket.Receive(recvBuffer);
                        if (received == 0)
                            break;

                        for (int i = 0; i < received; i++)
                            receivedData.Add(recvBuffer[i]);

                        if (received < recvBuffer.Length)
                            break;
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }

                RezultatLekar rezultatLekar;
                using (MemoryStream msReceive = new MemoryStream(receivedData.ToArray()))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    rezultatLekar = (RezultatLekar)bf.Deserialize(msReceive);
                }

                Pacijent p = pacijentRepozitorijum.PronadjiPoLBO(rezultatLekar.IdPacijenta);
                pacijentRepozitorijum.AzurirajStatusPacijenta(p);
                pacijentRepozitorijum.DodajObradjenogPacijenta(p);

                rezultatRepozitorijum.dodajRezultat(rezultatLekar);
                zahtevRepozitorijum.UkloniZavrsenZahtev(zahtev);

                Console.WriteLine($"[Server] Obrada zahteva završena: ");
                rezultatRepozitorijum.IspisiRezultat(rezultatLekar);

           
                jedinicaRepozitorijum.AzurirajStatus(jedinica);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server ERROR] Obrada zahteva: {ex.Message}");
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

                using (Socket lekarSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        IPEndPoint lekarEP = new IPEndPoint(IPAddress.Loopback, 6004);
                        lekarSocket.Connect(lekarEP);
                        Console.WriteLine("[Server] Konektovan na lekara.");

                        byte[] bufferToSend;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            var paket = Tuple.Create(pacijenti, rezultati);
                            bf.Serialize(ms, paket);
                            bufferToSend = ms.ToArray();
                        }

                        lekarSocket.Send(bufferToSend);
                        Console.WriteLine("[Server] Poslati podaci lekaru.");

                        using (MemoryStream msReceive = new MemoryStream())
                        {
                            byte[] bufferReceive = new byte[4096];
                            int bytesRead;

                            while ((bytesRead = lekarSocket.Receive(bufferReceive)) > 0)
                            {
                                msReceive.Write(bufferReceive, 0, bytesRead);
                                if (bytesRead < bufferReceive.Length)
                                    break; // kraj poruke
                            }

                            msReceive.Position = 0;
                            BinaryFormatter bf = new BinaryFormatter();
                            var odgovor = (Tuple<List<Pacijent>, List<RezultatLekar>>)bf.Deserialize(msReceive);

                            List<Pacijent> azuriraniPacijenti = odgovor.Item1;
                            List<RezultatLekar> obradjeniRezultati = odgovor.Item2;

                            Console.WriteLine($"[Server] Primljeno {azuriraniPacijenti.Count} ažuriranih pacijenata od lekara.");

                            KreirajNoveZahteveZaAzuriranePacijente(azuriraniPacijenti);

                            foreach (var rez in obradjeniRezultati)
                            {
                                rezultatRepozitorijum.ukloniRezultat(rez);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Server ERROR] Slanje lekaru: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server ERROR] Priprema podataka za lekara: {ex.Message}");
            }

            Thread.Sleep(20000);
        }
    }

    private static void KreirajNoveZahteveZaAzuriranePacijente(List<Pacijent> azuriraniPacijenti)
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
                if (pacijentAzuriran.Status == Status.CEKANJE_PREGLEDA||
                    pacijentAzuriran.Status == Status.CEKANJE_OPERACIJE||
         pacijentAzuriran.Status == Status.CEKANJE_TERAPIJE||
         pacijentAzuriran.Status == Status.OBAVLJENA_OPERACIJA||
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
