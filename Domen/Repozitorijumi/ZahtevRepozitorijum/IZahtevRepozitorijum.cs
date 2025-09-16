using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.ZahtevRepozitorijum
{
    public interface IZahtevRepozitorijum
    {
         void DodajZahtev(Zahtev zahtev);
         void UkloniZavrsenZahtev(Zahtev zahtev);
         Zahtev UzmiSledeciZahtevZaObradu();
        void AzurirajZahtev(Zahtev zahtev);
        void IspisiZahtev(Zahtev zahtev);
    }
}
