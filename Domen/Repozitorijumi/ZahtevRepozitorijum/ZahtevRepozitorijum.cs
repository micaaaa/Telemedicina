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
                // Prvo pronađi urgentne zahteve
                var urgentni = zahtevi
                    .Where(z =>
                    {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null && pac.VrsteZahteva.HasFlag(VrsteZahteva.URGENTA_POMOC);
                    })
                    .FirstOrDefault();

                if (urgentni != null)
                {
                    zahtevi.Remove(urgentni);
                    return urgentni;
                }

                // Ako nema urgentnih, uzmi prvi pregled ili terapiju po redosledu dodavanja (FIFO)
                var pregledIliTerapija = zahtevi
                    .Where(z =>
                    {
                        var pac = pacijentRepozitorijum.PronadjiPoLBO(z.IdPacijenta);
                        return pac != null &&
                            (pac.VrsteZahteva.HasFlag(VrsteZahteva.PREGLED) || pac.VrsteZahteva.HasFlag(VrsteZahteva.TERAPIJA));
                    })
                    .FirstOrDefault();

                if (pregledIliTerapija != null)
                {
                    zahtevi.Remove(pregledIliTerapija);
                }

                return pregledIliTerapija;
            }
        }

        // Dodatno, možeš dodati metodu da vidiš ima li još zahteva (za kontrolu loopa)
        public bool ImaZahteva()
        {
            lock (lockObj)
            {
                return zahtevi.Count > 0;
            }
        }
    }
}
