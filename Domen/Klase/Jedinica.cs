using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Klase
{
    [Serializable]
    public class Jedinica
    {
        public TipJedinice TipJedinice { get; set; }
        public int IdJedinice { get; set; }
        public StatusJedinice Status { get; set; }
        public Jedinica() { }
        public Jedinica(TipJedinice tipJedinice, int idJedinice, StatusJedinice status  )
        {
            TipJedinice = tipJedinice;
            IdJedinice = idJedinice;
            Status = status;
        }
    }
}
