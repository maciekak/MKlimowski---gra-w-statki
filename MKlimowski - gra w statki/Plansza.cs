using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Plansza
    {
        public List<Pole> ListaPol { get; }

        public Plansza()
        {
            ListaPol = new List<Pole>();
        }

        public void UstawNaStart()
        {
            for (int y = 0; y < MainWindow.Wymiar; y++)
            {
                for (int x = 0; x < MainWindow.Wymiar; x++)
                {
                    ListaPol.Add(new Pole(x, y));
                }
            }
        }

        public void Kopiuj(Plansza plansza)
        {
            ListaPol.ForEach(p => plansza.ListaPol.Add(p));
        }

        public bool UsunPole(int x, int y)
        {
            var itemToRemove = ListaPol.Single(p => p.PorownajPolozenie(x, y));
            return ListaPol.Remove(itemToRemove);
        }

        public bool UsunPoleIOkolice(int x, int y)
        {
            int iloscElementow = ListaPol.Count;

            var itemsToRemove = ListaPol.Where(p => p.CzyWOkolicy(x, y)).ToList();
            itemsToRemove.ForEach(p => ListaPol.Remove(p));

            return iloscElementow != ListaPol.Count;
        }


        public bool UstawStatek(int x, int y, int dlugosc, Kierunek kierunek)
        {
            //TODO: no kurwa popraw, bo chujowo
            List<Pole> pola;
            switch (kierunek)
            {
                case Kierunek.Dol:
                    pola = ListaPol.Where(p => p.X == x && p.Y >= y && p.Y < y + dlugosc).ToList();
                    break;
                case Kierunek.Prawo:
                    pola = ListaPol.Where(p => p.Y == y && p.X >= x && p.X < x + dlugosc).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kierunek), kierunek, null);
            }
            if (pola.Count != dlugosc) return false; //TODO: pola moze byc null?

            pola.ForEach(p => p.TypPola = RodzajPola.Statek);
            return true;

        }
    }
    public enum Kierunek
    {
        Dol, Prawo
    }
}
