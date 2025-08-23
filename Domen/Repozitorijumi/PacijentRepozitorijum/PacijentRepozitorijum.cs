using Domen.Enumeracije;
using Domen.Klase;
using Domen.PomocneMetode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Domen.Repozitorijumi.PacijentRepozitorijum
{
    public class PacijentRepozitorijum : IPacijentRepozitorijum
    {
        private static List<Pacijent> SviPacijenti = new List<Pacijent>();
        private static List<Pacijent> PregledaniPacijenti = new List<Pacijent>();

        // Ažurira status pacijenta i prebacuje ga u Pregledane ako je obrađen
        public void AzurirajStatusPacijenta(Pacijent pac)
        {
            Pacijent p = SviPacijenti.Find(x => x.LBO == pac.LBO);
            if (p != null)
            {
                switch (pac.Status)
                {
                    case Status.CEKANJE_OPERACIJE:
                        p.Status = Status.OBAVLJENA_OPERACIJA;
                        break;
                    case Status.CEKANJE_TERAPIJE:
                        p.Status = Status.OBAVLJENA_TERAPIJA;
                        break;
                    case Status.CEKANJE_PREGLEDA:
                        p.Status = Status.OBAVLJEN_PREGLED;
                        break;
                    default:
                        p.Status = pac.Status;
                        break;
                }

                if (p.Status == Status.OBAVLJENA_OPERACIJA || p.Status == Status.OBAVLJENA_TERAPIJA || p.Status == Status.OBAVLJEN_PREGLED)
                {
                    SviPacijenti.Remove(p);
                    PregledaniPacijenti.Add(p);
                }
            }
        }
        
        // Dodaje novog pacijenta u aktivne
        public void DodajPacijenta(Pacijent p)
        {
            SviPacijenti.Add(p);
        }

        // Ispisuje oba spiska - aktivne i pregledane
        public void ispisisSve()
        {
            Console.WriteLine("Aktivni pacijenti:");
            foreach (Pacijent p in SviPacijenti)
            {
                IspisUsluge iu = new IspisUsluge();
                IspisStatusa iss = new IspisStatusa();
                Console.WriteLine($"Pacijent: LBO: {p.LBO}, Ime: {p.Ime}, Prezime: {p.Prezime}, Adresa: {p.Adresa}, Vrsta zahteva: {iu.Ispisi(p.VrsteZahteva)}, Status: {iss.Ispisi(p.Status)}");
            }

            Console.WriteLine("\nPregledani pacijenti:");
            foreach (Pacijent p in PregledaniPacijenti)
            {
                IspisUsluge iu = new IspisUsluge();
                IspisStatusa iss = new IspisStatusa();
                Console.WriteLine($"Pacijent: LBO: {p.LBO}, Ime: {p.Ime}, Prezime: {p.Prezime}, Adresa: {p.Adresa}, Vrsta zahteva: {iu.Ispisi(p.VrsteZahteva)}, Status: {iss.Ispisi(p.Status)}");
            }
        }

        // Pronalaženje pacijenta po LBO u obe liste
        public Pacijent PronadjiPoLBO(int id)
        {
            Pacijent p = SviPacijenti.Find(x => x.LBO == id);
            if (p != null)
                return p;

            p = PregledaniPacijenti.Find(x => x.LBO == id);
            if (p != null)
                return p;

            return new Pacijent();
        }

        // Čuva obe liste u fajl
        public void SacuvajUFajl(Pacijent p)
        {
            string putanja = "pacijenti.dat";

            using (FileStream fs = new FileStream(putanja, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var sveListe = new Tuple<List<Pacijent>, List<Pacijent>>(SviPacijenti, PregledaniPacijenti);
                bf.Serialize(fs, sveListe);
            }
        }

  

        // Učitava obe liste iz fajla
        public void UcitajIzFajla()
        {
            string putanja = "pacijenti.dat";

            if (File.Exists(putanja))
            {
                using (FileStream fs = new FileStream(putanja, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var sveListe = (Tuple<List<Pacijent>, List<Pacijent>>)bf.Deserialize(fs);
                    SviPacijenti = sveListe.Item1 ?? new List<Pacijent>();
                    PregledaniPacijenti = sveListe.Item2 ?? new List<Pacijent>();
                }
            }
            else
            {
                SviPacijenti = new List<Pacijent>();
                PregledaniPacijenti = new List<Pacijent>();
            }
        }
    }
}
