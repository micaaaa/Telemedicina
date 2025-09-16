using Domen.Enumeracije;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Klase
{
    [Serializable]
    public class Pacijent
    {
        public int LBO { get; set; } 
        public String Ime { get; set; } = String.Empty;
        public String Prezime { get; set; } = String.Empty;
        public String Adresa { get; set; } = String.Empty;
        public VrsteZahteva VrsteZahteva { get; set; }
        public Status Status { get; set; }
        public Pacijent()
        {
        }
        public Pacijent(int lBO, string ime, string prezime, string adresa, VrsteZahteva vrsteZahteva, Status status)
        {
            LBO = lBO;
            Ime = ime;
            Prezime = prezime;
            Adresa = adresa;
            VrsteZahteva = vrsteZahteva;
            Status = status;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
