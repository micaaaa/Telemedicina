using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.JedinicaRepozitorijum
{
    public class JedinicaRepozitorijum : IJedinicaRepozitorijum
    {
        private List<Jedinica> jedinice = new List<Jedinica>();

        public JedinicaRepozitorijum()
        {
            jedinice.Add(new Jedinica { IdJedinice = 1, TipJedinice = TipJedinice.URGENTNA, Status = StatusJedinice.SLOBODNA });
            jedinice.Add(new Jedinica { IdJedinice = 2, TipJedinice = TipJedinice.DIJAGNOSTICKA, Status = StatusJedinice.SLOBODNA });
            jedinice.Add(new Jedinica { IdJedinice = 3, TipJedinice = TipJedinice.TERAPEUTSKA, Status = StatusJedinice.SLOBODNA });
        }
        public bool JeSlobodna(Jedinica jedinica)
        {
            
                return jedinica.Status == StatusJedinice.SLOBODNA;
            
        }
        public Jedinica PronadjiSlobodnuJedinicu(TipJedinice tip)
        {
            return jedinice.FirstOrDefault(j => j.TipJedinice == tip && j.Status == StatusJedinice.SLOBODNA);
        }

        public Jedinica GetById(int id)
        {
            return jedinice.FirstOrDefault(j => j.IdJedinice == id);
        }

        public void PostaviStatus(int id, StatusJedinice noviStatus)
        {
            var jedinica = GetById(id);
            if (jedinica != null)
                jedinica.Status = noviStatus;
        }

        public Jedinica PronadjiJedinicu(int id)
        {
            return jedinice.FirstOrDefault(j => j.IdJedinice == id);
        }
        public void AzurirajStatus(Jedinica pac)
        {
            Jedinica p = jedinice.Find(x => x.IdJedinice == pac.IdJedinice);
            if (p != null)
            {
                switch (pac.Status)
                {
                    case StatusJedinice.ZAUZETA:
                        p.Status = StatusJedinice.SLOBODNA;
                        break;
                    case StatusJedinice.SLOBODNA:
                        p.Status = StatusJedinice.ZAUZETA;
                        break;

                }


            }
        }
        public List<Jedinica> VratiSve()
        {
            return jedinice;
        }
        public  void IspisiJedinicu(Jedinica jedinica)
        {
            Console.WriteLine();
            Console.WriteLine("============================= JEDINICA =============================");
            Console.WriteLine("| ID jedinice |      Tip jedinice      |      Status jedinice      |");
            Console.WriteLine("|-------------|-----------------------|---------------------------|");
            Console.WriteLine($"| {jedinica.IdJedinice,-11} | {jedinica.TipJedinice,-21} | {jedinica.Status,-25} |");
            Console.WriteLine("=====================================================================");
        }
    }
}
