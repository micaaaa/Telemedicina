using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.RezultatRepozitorijum
{
    public interface IRezultatRepozitorijum
    {
   

        void dodajRezultat(RezultatLekar r);
        void ukloniRezultat(RezultatLekar r);
        RezultatLekar pronadjiRezultat(int id);
        List<RezultatLekar> vratiSve();
        void IspisiRezultat(RezultatLekar rezultat);
    }
}
