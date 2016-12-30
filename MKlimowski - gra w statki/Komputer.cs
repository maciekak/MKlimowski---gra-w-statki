using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Komputer : Uzytkownik
    {
        public static Pole LosujPole(Plansza planszaGracza)
        {
            var listaMozliwychPolDoStrzalu =
                planszaGracza.ListaPol.Where(p => p.TypPola == RodzajPola.Puste || p.TypPola == RodzajPola.Statek).ToList();

            var generator = new Random(new object().GetHashCode());
            int wylosowana = generator.Next(listaMozliwychPolDoStrzalu.Count());

            return listaMozliwychPolDoStrzalu[wylosowana];
        }
    }
}
