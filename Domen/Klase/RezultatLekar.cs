using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Klase
{
    [Serializable]
    public class RezultatLekar
    {
        public int IdPacijenta {  get; set; }
        public DateTime Vreme;
        public OpisRezultata OpisRezultata { get; set; }
        public RezultatLekar()
        {
        }

        public RezultatLekar(int idPacijenta, DateTime vreme, OpisRezultata opisRezultata)
        {
            IdPacijenta = idPacijenta;
            Vreme = vreme;
            OpisRezultata = opisRezultata;
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
