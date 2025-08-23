using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domen.Klase;
namespace Domen.Repozitorijumi.JedinicaRepozitorijum
{
    public class JedinicaRepozitorijum : IJedinicaRepozitorijum
    {
        private List<Jedinica> jedinice = new List<Jedinica>();

        public JedinicaRepozitorijum()
        {
            // Primer: Inicijalizuj neke jedinice
            jedinice.Add(new Jedinica { IdJedinice = 1, TipJedinice = TipJedinice.URGENTNA, Status = StatusJedinice.SLOBODNA });
            jedinice.Add(new Jedinica { IdJedinice = 2, TipJedinice = TipJedinice.DIJAGNOSTICKA, Status = StatusJedinice.SLOBODNA });
            jedinice.Add(new Jedinica { IdJedinice = 3, TipJedinice = TipJedinice.TERAPEUTSKA, Status = StatusJedinice.SLOBODNA });
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
    }
}
