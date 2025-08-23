using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public  class IspisUsluge
    {
        public String Ispisi(VrsteZahteva zah) {
            String zahtev="";
            switch (zah)
            {
                case VrsteZahteva.PREGLED:
                    zahtev = "pregled"; break;
                case VrsteZahteva.TERAPIJA:
                    zahtev = "terapija"; break;
                case VrsteZahteva.URGENTA_POMOC:
                    zahtev = "urgentna pomoc"; break;
            }
            return zahtev;
        }
    }
}
