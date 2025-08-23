using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class IspisStatusaZahteva
    {
        public String Ispisi(StatusZahteva stat)
        {
            String status = "";
            switch (stat)
            {
                case StatusZahteva.AKTIVAN:
                    status = "aktivan"; break;
                case StatusZahteva.U_OBRADI:
                    status = "u obradi"; break;
                case StatusZahteva.ZAVRSEN:
                    status = "zavrsen"; break;
            }
            return status;
        }
    }
}
