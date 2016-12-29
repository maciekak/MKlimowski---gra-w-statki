using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Gracz : Uzytkownik
    {
        public new void LosujStatki()
        {
            PlanszaUzytkownika.Zeruj();
            base.LosujStatki();

        }
    }
}
