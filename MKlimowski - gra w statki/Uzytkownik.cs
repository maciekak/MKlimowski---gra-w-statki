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
        public AkcjaPoStrzale OstatniaAkcja { get; set; }
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

        public AkcjaPoStrzale Strzal(int x, int y)
        {
            var pole = PlanszaUzytkownika.ListaPol.First(p => p.PorownajPolozenie(x, y));

            switch(pole.TypPola)
            {
                case RodzajPola.Puste:
                    pole.TypPola = RodzajPola.Pudlo;
                    return AkcjaPoStrzale.Pudlo;
                case RodzajPola.Pudlo:
                    return AkcjaPoStrzale.Blad;
                case RodzajPola.Trafiony:
                    return AkcjaPoStrzale.Blad;
                case RodzajPola.Statek:
                    pole.TypPola = RodzajPola.Trafiony;
                    return Zatop(pole) ? AkcjaPoStrzale.Zatopiony : AkcjaPoStrzale.Trafiony;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool Zatop(Pole pole)
        {
            var zbierzTrafionePola = new List<Pole> {pole};
            
            // Sprawdzanie czy na lewo jest statek
            var ostatniX = pole.X - 1;
            while (true)
            {
                var sasiedniePole =
                    PlanszaUzytkownika.ListaPol.FirstOrDefault(p => p.PorownajPolozenie(ostatniX, pole.Y));

                if (sasiedniePole == null || sasiedniePole.TypPola == RodzajPola.Pudlo ||
                    sasiedniePole.TypPola == RodzajPola.Puste) break;
                //Jesli jest nietrafione pole ze statkiem to mamy pewnosc, ze statek nie jest zatopiony - analogicznie w kazdym
                if (sasiedniePole.TypPola == RodzajPola.Statek) return false;
                zbierzTrafionePola.Add(sasiedniePole);
                ostatniX--;
            }

            // Sprawdzanie czy na prawo jest statek
            ostatniX = pole.X + 1;
            while (true)
            {
                var sasiedniePole =
                    PlanszaUzytkownika.ListaPol.FirstOrDefault(p => p.PorownajPolozenie(ostatniX, pole.Y));
                if (sasiedniePole == null || sasiedniePole.TypPola == RodzajPola.Pudlo ||
                    sasiedniePole.TypPola == RodzajPola.Puste) break;
                if (sasiedniePole.TypPola == RodzajPola.Statek) return false;
                zbierzTrafionePola.Add(sasiedniePole);
                ostatniX++;
            }

            // Sprawdzanie czy w gore jest statek
            int ostatniY = pole.Y - 1;
            while (true)
            {
                var sasiedniePole =
                    PlanszaUzytkownika.ListaPol.FirstOrDefault(p => p.PorownajPolozenie(pole.X, ostatniY));
                if (sasiedniePole == null || sasiedniePole.TypPola == RodzajPola.Pudlo ||
                    sasiedniePole.TypPola == RodzajPola.Puste) break;
                if (sasiedniePole.TypPola == RodzajPola.Statek) return false;
                zbierzTrafionePola.Add(sasiedniePole);
                ostatniY--;
            }

            // Sprawdzanie czy w dol jest statek
            ostatniY = pole.Y + 1;
            while (true)
            {
                var sasiedniePole =
                    PlanszaUzytkownika.ListaPol.FirstOrDefault(p => p.PorownajPolozenie(pole.X, ostatniY));
                if (sasiedniePole == null || sasiedniePole.TypPola == RodzajPola.Pudlo ||
                    sasiedniePole.TypPola == RodzajPola.Puste) break;
                if (sasiedniePole.TypPola == RodzajPola.Statek) return false;
                zbierzTrafionePola.Add(sasiedniePole);
                ostatniY++;
            }

            var statek = Statki.First(s => zbierzTrafionePola.Exists(p => s.X == p.X && s.Y == p.Y));
            statek.CzyZatopiony = true;

            var okolica = PlanszaUzytkownika.ZnajdzOkolice(zbierzTrafionePola);

            //Ustawia wszystkie sasiednie pola statku jako pudla
            okolica.ForEach(p => p.TypPola = RodzajPola.Pudlo);
            return true;
        }

        public bool CzyKoniec()
        {
            //Jesli nie ma zadnego pola ze statkiem to zwraca prawde
            return !PlanszaUzytkownika.ListaPol.Exists(p => p.TypPola == RodzajPola.Statek);
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

    public enum AkcjaPoStrzale
    {
        Trafiony, Pudlo, Zatopiony, Blad
    }
}
