using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class IspisPacijenta
    {
        public void ispisiPacijenta(Pacijent p) {
            IspisUsluge iu= new IspisUsluge();
            IspisStatusa iss = new IspisStatusa();
            Console.WriteLine($"Pacijent: LBO: {p.LBO}, Ime: {p.Ime}, Prezime: {p.Prezime}, Adresa: {p.Adresa}, Vrsta zahteva: {iu.Ispisi(p.VrsteZahteva)},Status: {iss.Ispisi(p.Status)}");
        }
    }
}
