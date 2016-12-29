using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Pole
    {
        public int X { get; }
        public int Y { get; }
        public RodzajPola TypPola { get; set; }
        public Pole(int x, int y, RodzajPola typPola = RodzajPola.Puste)
        {
            X = x;
            Y = y;
            TypPola = typPola;
        }

        public bool PorownajPolozenie(int x, int y)
        {
            return X == x && Y == y;
        }

        public bool PorownajPole(Pole pole)
        {
            return X == pole.X && Y == pole.Y && pole.TypPola == TypPola;
        }

        public bool CzyWOkolicy(int x, int y)
        {
            return X >= x - 1 && X <= x + 1 && Y >= y - 1 && Y <= y + 1;
        }
    }

    public enum RodzajPola
    {
        Puste, Pudlo, Trafiony, Statek
    }
}
