using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Klase
{
    public class Zahtev
    {

        public int IdPacijenta { get; set; }
        public int IdJedinice {  get; set; }
        public StatusZahteva StatusZahteva { get; set;}
        public Zahtev()
        {
        }
        public Zahtev(int idPacijenta, int idJedinice, StatusZahteva statusZahteva)
        {
            IdPacijenta = idPacijenta;
            IdJedinice = idJedinice;
            StatusZahteva = statusZahteva;
        }
    }
}
