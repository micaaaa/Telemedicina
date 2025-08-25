using Domen.Enumeracije;
using Domen.Klase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domen.Repozitorijumi.JedinicaRepozitorijum
{
    public interface IJedinicaRepozitorijum
    {
        Jedinica PronadjiJedinicu(int id);
        
    }
}
