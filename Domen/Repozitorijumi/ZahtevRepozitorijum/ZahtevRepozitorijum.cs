using Domen.Enumeracije;
using Domen.Klase;
using Domen.Repozitorijumi.PacijentRepozitorijum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domen.Repozitorijumi.ZahtevRepozitorijum
{
    public class ZahtevRepozitorijum : IZahtevRepozitorijum
    {
        private readonly List<Zahtev> zahtevi = new List<Zahtev>();
        private readonly IPacijentRepozitorijum pacijentRepozitorijum;
        private readonly object lockObj = new object();  // Za thread-safety

        public ZahtevRepozitorijum(IPacijentRepozitorijum pacijentRepozitorijum)
        {
            this.pacijentRepozitorijum = pacijentRepozitorijum;
        }

        public void DodajZahtev(Zahtev zahtev)
        {
            lock (lockObj)
            {
                zahtevi.Add(zahtev);
            }
        }

        public void UkloniZavrsenZahtev(Zahtev zahtev)
        {
            lock (lockObj)
            {
                zahtevi.Remove(zahtev);
            }
        }

        public Zahtev UzmiSledeciZahtevZaObradu()
        {
            lock (lockObj)
            {
                // 1. Pronađi urgentne
                var urgentni = zahtevi
                    .FirstOrDefault(z =>
                    {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null && pac.VrsteZahteva == VrsteZahteva.URGENTA_POMOC;
                    });

                if (urgentni != null)
                {
                    zahtevi.Remove(urgentni);
                    return urgentni;
                }

                // 2. Ako nema urgentnih, traži pregled ili terapiju
                var pregledIliTerapija = zahtevi
                    .FirstOrDefault(z =>
                    {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null &&
                               (pac.VrsteZahteva == VrsteZahteva.PREGLED || pac.VrsteZahteva == VrsteZahteva.TERAPIJA);
                    });

                if (pregledIliTerapija != null)
                {
                    zahtevi.Remove(pregledIliTerapija);
                    return pregledIliTerapija;
                }

                return null;
            }
        }


        public bool ImaZahteva()
        {
            lock (lockObj)
            {
                return zahtevi.Count > 0;
            }
        }
    }
}
