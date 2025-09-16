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
        private static List<Pacijent> obradjeniPacijenti=new List<Pacijent>();
        static object lockrep = new object();



        public void AzurirajStatusPacijenta(Pacijent pac)
        {
            Pacijent p = SviPacijenti.Find(x => x.LBO == pac.LBO);
            if (p != null)
            {
                switch (pac.Status)
                {
                    case Status.CEKANJE_OPERACIJE:
                        p.Status = Status.OPERACIJA_U_TOKU;
                        break;
                    case Status.CEKANJE_TERAPIJE:
                        p.Status = Status.TERAPIJA_U_TOKU;
                        break;
                    case Status.CEKANJE_PREGLEDA:
                        p.Status = Status.PREGLED_U_TOKU;
                        break;
                    case Status.PREGLED_U_TOKU:
                        p.Status = Status.OBAVLJEN_PREGLED;
                        break;
                    case Status.OPERACIJA_U_TOKU:
                        p.Status = Status.OBAVLJENA_OPERACIJA;
                        break;
                    case Status.TERAPIJA_U_TOKU:
                        p.Status = Status.OBAVLJENA_TERAPIJA;

                        break;
                    default:
                        p.Status = pac.Status;
                        break;
                }


            }
        }

        public void DodajPacijenta(Pacijent p)
        {
            lock (lockrep)
            {
                SviPacijenti.Add(p);
            }
        }
        public void DodajObradjenogPacijenta(Pacijent p)
        {
            obradjeniPacijenti.Add(p);
        }

        public void ispisisSve()
        {
            lock (lockrep)
            {
                Console.WriteLine("Aktivni pacijenti:");
                foreach (Pacijent p in SviPacijenti)
                {
                    IspisUsluge iu = new IspisUsluge();
                    IspisStatusa iss = new IspisStatusa();
                    Console.WriteLine($"Pacijent: LBO: {p.LBO}, Ime: {p.Ime}, Prezime: {p.Prezime}, Adresa: {p.Adresa}, Vrsta zahteva: {iu.Ispisi(p.VrsteZahteva)}, Status: {iss.Ispisi(p.Status)}");
                }
            }
           
        }
        public List<Pacijent> VratiSve() {
            lock (lockrep)
            {
              
                return SviPacijenti;
            }
        }
        public List<Pacijent> VratiSveObradjene()
        {
            return obradjeniPacijenti;
        }

        public Pacijent PronadjiPoLBO(int id)
        {
            Pacijent p = SviPacijenti.Find(x => x.LBO == id);
            if (p != null)
                return p;


            return new Pacijent();
        }


        public void SacuvajUFajl()
        {
            string putanja = "pacijenti.dat";

            using (FileStream fs = new FileStream(putanja, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var sveListe = new Tuple<List<Pacijent>>(SviPacijenti);
                bf.Serialize(fs, sveListe);
            }
        }

        public List<Pacijent> UcitajIzFajla()
        {
            lock (lockrep)
            {
                string putanja = "pacijenti.dat";

                if (!File.Exists(putanja))
                {
                    Console.WriteLine("[Repozitorijum] Fajl sa pacijentima ne postoji. Kreira se prazan repozitorijum.");
                    SviPacijenti = new List<Pacijent>();
                    return SviPacijenti;
                }

                try
                {
                    using (FileStream fs = new FileStream(putanja, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        var sveListe = (Tuple<List<Pacijent>>)bf.Deserialize(fs);
                        SviPacijenti = sveListe.Item1 ?? new List<Pacijent>();


                    }
                    return SviPacijenti;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Repozitorijum ERROR] Neuspešno učitavanje pacijenata: {ex.Message}");
                    SviPacijenti = new List<Pacijent>();
                    return SviPacijenti;
                }
            }
        }
        public void UcitajIzFajlaObradjene()
        {
            string putanja = "pacijentiobradjeni.dat";

            if (!File.Exists(putanja))
            {
                Console.WriteLine("[Repozitorijum] Fajl sa pacijentima ne postoji. Kreira se prazan repozitorijum.");
                SviPacijenti = new List<Pacijent>();
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(putanja, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    var sveListe = (Tuple<List<Pacijent>>)bf.Deserialize(fs);
                    obradjeniPacijenti = sveListe.Item1 ?? new List<Pacijent>();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Repozitorijum ERROR] Neuspešno učitavanje pacijenata: {ex.Message}");
                obradjeniPacijenti = new List<Pacijent>(); 
            }
        }
        public void SacuvajUFajlObradjene()
        {
            string putanja = "pacijentiobradjeni.dat";

            using (FileStream fs = new FileStream(putanja, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var sveListe = new Tuple<List<Pacijent>>(obradjeniPacijenti);
                bf.Serialize(fs, sveListe);
            }
        }
        public void UkloniObradjenog(Pacijent p)
        {
            lock (lockrep)
            {
                obradjeniPacijenti.Remove(p);
            }
           
        }
        public void UkloniObradjenog2(Pacijent p)
        {
            SviPacijenti.Remove(p);
            SacuvajUFajl();

        }
        public void IspisiPacijenta(Pacijent pacijent)
        {

            Console.WriteLine();
            Console.WriteLine("================================ PACIJENT ==================================");
            Console.WriteLine("| LBO   | Ime        | Prezime     | Vrsta zahteva     | Status           |");
            Console.WriteLine("|-------|------------|-------------|--------------------|------------------|");
            Console.WriteLine($"| {pacijent.LBO,-5} | {pacijent.Ime,-10} | {pacijent.Prezime,-11} | {pacijent.VrsteZahteva,-18} | {pacijent.Status,-16} |");
            Console.WriteLine("============================================================================");
            Console.WriteLine();
        }



    }
}
