using Domen.Enumeracije;
using System;
using Domen.Klase;

namespace Domen.PomocneMetode
{
    public class RegistracijaPacijenta
    {
        public Pacijent Registracija()
        {
            int lbo;
            string ime, prezime, adresa;
            VrsteZahteva zahtev = new VrsteZahteva();

            Console.WriteLine("Dobro došli! Unesite Vaše podatke:");

            // LBO
            while (true)
            {
                Console.Write("LBO: ");
                if (int.TryParse(Console.ReadLine(), out lbo) && lbo > 0)
                    break;
                Console.WriteLine("Greška: Unesite validan pozitivan broj za LBO.");
            }

            // Ime
            while (true)
            {
                Console.Write("Ime: ");
                ime = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(ime))
                    break;
                Console.WriteLine("Greška: Ime ne može biti prazno.");
            }

            // Prezime
            while (true)
            {
                Console.Write("Prezime: ");
                prezime = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(prezime))
                    break;
                Console.WriteLine("Greška: Prezime ne može biti prazno.");
            }

            // Adresa
            while (true)
            {
                Console.Write("Adresa: ");
                adresa = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(adresa))
                    break;
                Console.WriteLine("Greška: Adresa ne može biti prazna.");
            }

            // Vrsta zahteva
            int zah;
            while (true)
            {
                Console.WriteLine("Izaberite jednu od opcija:");
                Console.WriteLine("1. Pregled");
                Console.WriteLine("2. Terapija");
                Console.WriteLine("3. Urgentna pomoć");
                Console.Write("Opcija [1 - 3]: ");

                if (int.TryParse(Console.ReadLine(), out zah) && zah >= 1 && zah <= 3)
                {
                    switch (zah)
                    {
                        case 1: zahtev = VrsteZahteva.PREGLED; break;
                        case 2: zahtev = VrsteZahteva.TERAPIJA; break;
                        case 3: zahtev = VrsteZahteva.URGENTA_POMOC; break;
                    }
                    break;
                }
                Console.WriteLine("Greška: Morate uneti broj između 1 i 3.");
            }

            // Postavljanje početnog statusa na osnovu zahteva
            Status status = new Status();
            switch (zahtev)
            {
                case VrsteZahteva.TERAPIJA:
                    status = Status.CEKANJE_TERAPIJE; break;
                case VrsteZahteva.PREGLED:
                    status = Status.CEKANJE_PREGLEDA; break;
                case VrsteZahteva.URGENTA_POMOC:
                    status = Status.CEKANJE_OPERACIJE; break;
            }

            Pacijent p = new Pacijent(lbo, ime, prezime, adresa, zahtev, status);
            return p;
        }
    }
}
