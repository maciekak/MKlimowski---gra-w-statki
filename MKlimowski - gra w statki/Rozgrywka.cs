using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Rozgrywka
    {
        public Gracz Player { get; set; }
        public Komputer PrzeciwnikKomputerowy { get; set; }

        public Rozgrywka()
        {
            Player = new Gracz();
            PrzeciwnikKomputerowy = new Komputer();
        }


        public void InicjalizujFakowymiStatkami()
        {
            Plansza.UstawStatek(Player.PlanszaUzytkownika.ListaPol, new Statek(4) {X = 2, Y = 2, Kierunek = Kierunek.Dol});
            Plansza.UstawStatek(Player.PlanszaUzytkownika.ListaPol, new Statek(3) { X = 4, Y = 4, Kierunek = Kierunek.Prawo });
            Plansza.UstawStatek(Player.PlanszaUzytkownika.ListaPol, new Statek(2) { X = 6, Y = 6, Kierunek = Kierunek.Dol });
            Plansza.UstawStatek(Player.PlanszaUzytkownika.ListaPol, new Statek(1) { X = 8, Y = 8, Kierunek = Kierunek.Prawo });
        }
    }
}
