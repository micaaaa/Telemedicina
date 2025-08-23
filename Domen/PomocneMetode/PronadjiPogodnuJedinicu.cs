using Domen.Enumeracije;
using Domen.Klase;
using Domen.Repozitorijumi.JedinicaRepozitorijum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class PronadjiPogodnuJedinicu
    {
        public Jedinica PronadjiPogodnu(VrsteZahteva vrstaZahteva, JedinicaRepozitorijum repozitorijum)
        {
            TipJedinice trazeniTip;

            switch (vrstaZahteva)
            {
                case VrsteZahteva.URGENTA_POMOC:
                    trazeniTip = TipJedinice.URGENTNA;
                    break;

                case VrsteZahteva.PREGLED:
                    trazeniTip = TipJedinice.DIJAGNOSTICKA;
                    break;

                case VrsteZahteva.TERAPIJA:
                    trazeniTip = TipJedinice.TERAPEUTSKA;
                    break;

                default:
                    throw new ArgumentException("Nepoznata vrsta zahteva: " + vrstaZahteva);
            }

            return repozitorijum.PronadjiSlobodnuJedinicu(trazeniTip);
        }
    }
}
