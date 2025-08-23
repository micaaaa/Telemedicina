using Domen.Enumeracije;
using Domen.Klase;
using Domen.PomocneMetode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.PacijentRepozitorijum
{
    public class PacijentRepozitorijum : IPacijentRepozitorijum
    {
        private static List<Pacijent> SviPacijenti = new List<Pacijent>();

        public void AzurirajStatusPacijenta(Pacijent pac)
        {
            foreach (Pacijent p in SviPacijenti) {
                if (p.LBO == pac.LBO) {
                    switch (pac.Status)
                    {
                        case Status.CEKANJE_OPERACIJE:
                            p.Status=Status.OBAVLJENA_OPERACIJA; break;
                        case Status.CEKANJE_TERAPIJE:
                             p.Status = Status.OBAVLJENA_TERAPIJA; break;
                        case Status.CEKANJE_PREGLEDA:
                            p.Status = Status.OBAVLJEN_PREGLED; break;

                    }

                }
            }
        }

        public void DodajPacijenta(Pacijent p) {
            SviPacijenti.Add(p);
        }

        public void ispisisSve()
        {
            foreach (Pacijent p in SviPacijenti)
            {
                IspisUsluge iu = new IspisUsluge();
                IspisStatusa iss = new IspisStatusa();
                Console.WriteLine($"Pacijent: LBO: {p.LBO}, Ime: {p.Ime}, Prezime: {p.Prezime}, Adresa: {p.Adresa}, Vrsta zahteva: {iu.Ispisi(p.VrsteZahteva)},Status: {iss.Ispisi(p.Status)}");
            }
        }


        public Pacijent PronadjiPoLBO(int id)
        {
            foreach (Pacijent p in SviPacijenti)
            {
                if (p.LBO == id)
                {
                    return p;
                }
            }
            return new Pacijent();
        }

        public void SacuvajUFajl(Pacijent p)
        {
            string putanja = "pacijenti.dat";

            List<Pacijent> pacijenti;

            if (File.Exists(putanja))
            {
                // Učitaj postojeće pacijente
                using (FileStream fs = new FileStream(putanja, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    pacijenti = (List<Pacijent>)bf.Deserialize(fs);
                }
            }
            else
            {
                pacijenti = new List<Pacijent>();
            }

            // Dodaj novog pacijenta
            pacijenti.Add(p);

            // Sačuvaj nazad u fajl
            using (FileStream fs = new FileStream(putanja, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, pacijenti);
            }
        }

        public void UcitajIzFajla()
        {
            string putanja = "pacijenti.dat";

            if (File.Exists(putanja))
            {
                using (FileStream fs = new FileStream(putanja, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    SviPacijenti = (List<Pacijent>)bf.Deserialize(fs);
                }
            }
            else
            {
                SviPacijenti = new List<Pacijent>();
            }
        }
    }
}
