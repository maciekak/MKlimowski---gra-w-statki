using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Statek : IPolozenie
    {
        public int Dlugosc { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public Kierunek Kierunek { get; set; }
        public bool CzyUstawiony { get; set; }
        public bool CzyZatopiony { get; set; }

        public Statek(int dlugosc)
        {
            Dlugosc = dlugosc;
            CzyUstawiony = false;
            CzyZatopiony = false;
        }
    }
}
