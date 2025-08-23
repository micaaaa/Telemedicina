using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.PomocneMetode
{
    public class IspisStatusa
    {
        public String Ispisi(Status stat)
        {
            String status = "";
            switch (stat)
            {
                case Status.CEKANJE_OPERACIJE:
                    status = "cekanje_operacije"; break;
                case Status.CEKANJE_TERAPIJE:
                    status = "cekanje terapije"; break;
                case Status.CEKANJE_PREGLEDA:
                    status = "cekanje pregleda"; break;
            }
            return status;
        }
    }
}
