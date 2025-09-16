using Domen.Enumeracije;
using Domen.Klase;
using System.Collections.Generic;

namespace Domen.Repozitorijumi.JedinicaRepozitorijum
{
    public interface IJedinicaRepozitorijum
    {
        Jedinica PronadjiJedinicu(int id);
        Jedinica PronadjiSlobodnuJedinicu(TipJedinice tip);
        Jedinica GetById(int id);
        List<Jedinica> VratiSve();
        bool JeSlobodna(Jedinica jedinica);
        void PostaviStatus(int id, StatusJedinice noviStatus);
         void IspisiJedinicu(Jedinica jedinica);
        void AzurirajStatus(Jedinica pac);
    }
}
