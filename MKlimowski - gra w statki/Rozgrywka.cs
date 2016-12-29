using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Rozgrywka
    {
        public Plansza Gracza { get; set; }
        public Plansza Komputera { get; set; }

        public Rozgrywka()
        {
            Gracza = new Plansza();
            Komputera = new Plansza();
            Gracza.UstawNaStart();
            Komputera.UstawNaStart();
        }


        public void InicjalizujFakowymiStatkami()
        {
            Gracza.UstawStatek(2,2,4, Kierunek.Dol);
            Gracza.UstawStatek(4,4,3, Kierunek.Dol);
            Gracza.UstawStatek(6,6,2, Kierunek.Prawo);
            Gracza.UstawStatek(8, 8, 1, Kierunek.Dol);
        }
    }

}
