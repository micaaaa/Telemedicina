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
                // 1. Uzmi prvi urgentni zahtev ako postoji
                var urgentniZahtevi = zahtevi
                    .Where(z => {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null && pac.VrsteZahteva == VrsteZahteva.URGENTA_POMOC;
                    })
                    .ToList();

                if (urgentniZahtevi.Count > 0)
                {
                    var prviUrgentni = urgentniZahtevi[0];
                    zahtevi.Remove(prviUrgentni);
                    return prviUrgentni;
                }

                // 2. Uzmi prvi zahtev koji je PREGLED ili TERAPIJA
                var pregledIliTerapija = zahtevi
                    .FirstOrDefault(z => {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null &&
                               (pac.VrsteZahteva == VrsteZahteva.PREGLED || pac.VrsteZahteva == VrsteZahteva.TERAPIJA);
                    });

                if (pregledIliTerapija != null)
                {
                    zahtevi.Remove(pregledIliTerapija);
                    return pregledIliTerapija;
                }

                // 3. Na kraju, uzmi prvi zahtev bilo kog drugog tipa (ostali)
                if (zahtevi.Count > 0)
                {
                    var prvi = zahtevi[0];
                    zahtevi.RemoveAt(0);
                    return prvi;
                }

                // Nema zahteva
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
