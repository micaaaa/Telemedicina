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
    }
}
