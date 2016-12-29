using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public abstract class Uzytkownik
    {
        public const int IloscStatkow = 4;
        public List<Statek> Statki { get; set; }
        public Plansza PlanszaUzytkownika { get; set; }
        public void LosujStatki()
        {
            var listaMozliwychPol = new List<Pole>();
            Plansza.Kopiuj(PlanszaUzytkownika.ListaPol, listaMozliwychPol);

            foreach (var statek in Statki)
            {
                do
                {
                    var generator = new Random(new object().GetHashCode());
                    int wylosowana = generator.Next(listaMozliwychPol.Count);

                    statek.Kierunek = (Kierunek) generator.Next(2);
                    statek.X = listaMozliwychPol[wylosowana].X;
                    statek.Y = listaMozliwychPol[wylosowana].Y;
                }
                while (!Plansza.UstawStatek(listaMozliwychPol, statek));

                statek.CzyUstawiony = true;
                Plansza.UsunStatekIOkolice(listaMozliwychPol, statek);
            }
        }

        protected Uzytkownik()
        {
            PlanszaUzytkownika = new Plansza();
            PlanszaUzytkownika.UstawNaStart();
            Statki = new List<Statek>();
            for (int i = IloscStatkow; i >= 1; i--)
            {
                for (int j = IloscStatkow; j >= i; j--)
                {
                    Statki.Add(new Statek(i));
                }
            }
        }
    }
}
