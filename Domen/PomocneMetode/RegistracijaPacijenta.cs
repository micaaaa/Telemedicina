using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class RegistracijaPacijenta
    {
        public void Registracija()
        {
            int lbo;
            int zah;
            string ime, prezime, adresa;
            VrsteZahteva zahtev;
            Console.WriteLine("Doslo dosli! Unesite Vase podatke:");
            Console.WriteLine("LBO: ");
            lbo = int.Parse(Console.ReadLine());
            Console.WriteLine("Ime: ");
            ime = Console.ReadLine();
            Console.WriteLine("Prezime: ");
            prezime = Console.ReadLine();
            Console.WriteLine("Adresa: ");
            adresa = Console.ReadLine();
            Console.WriteLine("Izaberite 1 od opcija:\n 1. Pregled\n 2. Terapija\n 3. Urgenta pomoc\n Opcija[1 - 3]: ");
            zah = int.Parse(Console.ReadLine());
            switch(zah)
            {
                case 1:
                    zahtev = VrsteZahteva.PREGLED; break;
                case 2:
                    zahtev = VrsteZahteva.TERAPIJA; break;
                case 3:
                    zahtev = VrsteZahteva.URGENTA_POMOC; break;
                default:
                    Console.WriteLine("Molim Vas unesite broj od ponudjenih opcija 1, 2 ili 3!"); break;
            }

        }
    }
}
