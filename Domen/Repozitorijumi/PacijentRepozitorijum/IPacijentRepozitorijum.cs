using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.PacijentRepozitorijum
{
    public interface IPacijentRepozitorijum
    {
        Pacijent PronadjiPoLBO(int id);
        void DodajPacijenta(Pacijent p);
        void AzurirajStatusPacijenta(Pacijent p);
        void ispisisSve();
       void SacuvajUFajl(Pacijent p);
        void UcitajIzFajla();
        List<Pacijent> VratiSve();
        void DodajObradjenogPacijenta(Pacijent p);
        List<Pacijent> VratiSveObradjene();
        void UkloniObradjenog(Pacijent p);
    }
}
