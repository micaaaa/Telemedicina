using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Klase
{
    public class ZahtevPacijent
    {

        [Serializable]
        public class ZahtevPacijentDTO
        {
            public Pacijent Pacijent { get; set; }
            public Zahtev Zahtev { get; set; }

            public ZahtevPacijentDTO(Pacijent pacijent, Zahtev zahtev)
            {
                Pacijent = pacijent;
                Zahtev = zahtev;
            }

            public ZahtevPacijentDTO() { }
        }
    }

}

