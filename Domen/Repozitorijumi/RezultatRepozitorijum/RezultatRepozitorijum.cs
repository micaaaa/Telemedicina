using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.RezultatRepozitorijum
{
    public class RezultatRepozitorijum : IRezultatRepozitorijum
    {
        List<RezultatLekar> rezultati = new List<RezultatLekar>();
        private readonly object lockObj = new object();
        public void dodajRezultat(RezultatLekar r)
        {
            rezultati.Add(r);
        }

        public void ukloniRezultat(RezultatLekar r)
        {
         
        
            lock (lockObj)
            {
                var postoji = rezultati.Find(r1 => r.IdPacijenta == r.IdPacijenta);
                if (postoji != null)
                {
                    rezultati.Remove(postoji);
                }
            }
        
        }
        public RezultatLekar pronadjiRezultat(int id) {
            foreach (RezultatLekar r in rezultati) {
                if (r.IdPacijenta == id)
                    return r;
            }
            return new RezultatLekar();
         }

        public List<RezultatLekar> vratiSve()
        {
            return rezultati;
        }
        public void IspisiRezultat(RezultatLekar rezultat)
        {
            Console.WriteLine();
            Console.WriteLine("========================== REZULTAT LEKARA ==========================");
            Console.WriteLine("| ID pacijenta |      Vreme       |       Opis rezultata         |");
            Console.WriteLine("|--------------|------------------|------------------------------|");
            Console.WriteLine($"| {rezultat.IdPacijenta,-12} | {rezultat.Vreme,-16:yyyy-MM-dd HH:mm} | {rezultat.OpisRezultata,-28} |");
            Console.WriteLine("=====================================================================");
        }

    }
}
