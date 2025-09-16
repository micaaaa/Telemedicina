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
        private readonly object lockObj = new object(); 

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

                if (zahtevi.Count > 0)
                {
                    var prvi = zahtevi[0];
                    zahtevi.RemoveAt(0);
                    return prvi;
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
        public void AzurirajZahtev(Zahtev zahtev)
        {
            lock (lockObj)
            {
                int index = zahtevi.FindIndex(z => z.IdPacijenta == zahtev.IdPacijenta);
                if (index != -1)
                {
                    // Ažuriraj postojeći zahtev
                    zahtevi[index] = zahtev;
                    Console.WriteLine($"[Repozitorijum] Zahtev pacijenta {zahtev.IdPacijenta} ažuriran.");
                }
                else
                {
                    // Dodaj novi zahtev ako nije postojao
                    zahtevi.Add(zahtev);
                    Console.WriteLine($"[Repozitorijum] Zahtev pacijenta {zahtev.IdPacijenta} nije postojao, dodat je novi.");
                }
            }
        }

        public List<Zahtev> VratiSve()
        {
            return zahtevi;
        }

        public void IspisiZahtev(Zahtev zahtev)
        {
            Console.WriteLine();
            Console.WriteLine("============================ ZAHTEV KREIRAN ============================");
            Console.WriteLine("| Pacijent ID | Jedinica ID |     Status zahteva     |");
            Console.WriteLine("|-------------|-------------|-------------------------|");
            Console.WriteLine($"| {zahtev.IdPacijenta,-11} | {zahtev.IdJedinice,-11} | {zahtev.StatusZahteva,-23} |");
            Console.WriteLine("========================================================================");
        }

    }
}
