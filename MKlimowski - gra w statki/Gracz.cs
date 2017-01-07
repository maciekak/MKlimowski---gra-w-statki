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

        public void Resetuj()
        {
            //Reset Statkow
            Statki = Statki.Select(s => new Statek(s.Dlugosc)).ToList();

            //Reset Planszy
            PlanszaUzytkownika.Zeruj();
        }

        public bool CzyUstawiono()
        {
            return !Statki.Exists(x => x.CzyUstawiony == false);
        }
    }
}
