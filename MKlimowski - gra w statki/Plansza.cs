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

        public void Zeruj()
        {
            ListaPol.ForEach(p => p.TypPola = RodzajPola.Puste);
        }

        public static void Kopiuj(List<Pole> zListy, List<Pole> doListy)
        {
            zListy.ForEach(doListy.Add);
        }

        public static bool UsunPole(int x, int y, List<Pole> listaPol )
        {
            var itemToRemove = listaPol.Single(p => p.PorownajPolozenie(x, y));
            return listaPol.Remove(itemToRemove);
        }

        public List<Pole> ZnajdzOkolice(List<Pole> pola)
        {
            var okolica = pola.SelectMany(p => ListaPol.Where(a => a.CzyWOkolicy(p.X, p.Y))).ToList();

            okolica = okolica.Distinct().ToList();
            okolica.RemoveAll(pola.Contains);

            return okolica;
        }

        public bool UsunPoleIOkolice(int x, int y)
        {
            return ListaPol.Count == ListaPol.RemoveAll(p => p.CzyWOkolicy(x, y));
        }

        public static bool UsunStatekIOkolice(List<Pole> polaPlanszy, Statek statek)
        {
            //TODO: gownokod przerobic z wykorzystaniem powyzszej metody
            int dlugoscPierwotnaListy = polaPlanszy.Count;
            List<Pole> pola;
            switch (statek.Kierunek)
            {
                case Kierunek.Dol:
                    pola =
                        polaPlanszy.Where( p => p.X >= statek.X - 1 && p.X <= statek.X + 1 && p.Y >= statek.Y - 1 && p.Y < statek.Y + statek.Dlugosc + 1).ToList();
                    break;
                case Kierunek.Prawo:
                    pola =
                        polaPlanszy.Where(
                            p =>
                                p.Y >= statek.Y - 1 && p.Y <= statek.Y + 1 && p.X >= statek.X - 1 &&
                                p.X < statek.X + statek.Dlugosc + 1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            pola.ForEach(p => polaPlanszy.Remove(p));

            return polaPlanszy.Count + pola.Count == dlugoscPierwotnaListy;
        }

        public static bool UstawStatek(List<Pole> startowePola, Statek statek)
        {
            //TODO: no kurwa popraw, bo chujowo
            List<Pole> pola;
            switch (statek.Kierunek)
            {
                case Kierunek.Dol:
                    pola = startowePola.Where(p => p.X == statek.X && p.Y >= statek.Y && p.Y < statek.Y + statek.Dlugosc && p.TypPola == RodzajPola.Puste).ToList();
                    break;
                case Kierunek.Prawo:
                    pola = startowePola.Where(p => p.Y == statek.Y && p.X >= statek.X && p.X < statek.X + statek.Dlugosc && p.TypPola == RodzajPola.Puste).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statek.Kierunek), statek.Kierunek, null);
            }
            if (pola.Count != statek.Dlugosc) return false;

            pola.ForEach(p => p.TypPola = RodzajPola.Statek);
            return true;
        }
    }
    public enum Kierunek
    {
        Dol, Prawo
    }
}
