using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class Komputer : Uzytkownik
    {
        private Pole OstatniePole { get; set; }
        private LogikaStrzelania OstatniTypStrzelania { get; set; }
        private Kierunek OstatniKierunek { get; set; }


        public void UstawOstatniTypStrzelania(AkcjaPoStrzale akcja)
        {
            switch (akcja)
            {
                case AkcjaPoStrzale.Trafiony:
                    OstatniTypStrzelania = OstatniTypStrzelania == LogikaStrzelania.Normalny
                        ? LogikaStrzelania.Trafiony
                        : LogikaStrzelania.ZKierunkiem;
                    break;
                case AkcjaPoStrzale.Pudlo:
                    //Nic nie robimy, bo jesli bylo 'trafiony' to dalej strzela w ten statek
                    //jesli byl 'normalny' to dalej bedzie celowal po calej planszy
                    break;
                case AkcjaPoStrzale.Zatopiony:
                    OstatniTypStrzelania = LogikaStrzelania.Normalny;
                    break;
                case AkcjaPoStrzale.Blad:
                    OstatniTypStrzelania = LogikaStrzelania.Normalny;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(akcja), akcja, null);
            }
        }
        public Pole LosujPole(Plansza planszaGracza)
        {
            var listaMozliwychPolDoStrzalu =
                planszaGracza.ListaPol.Where(p => p.TypPola == RodzajPola.Puste || p.TypPola == RodzajPola.Statek).ToList();
            var generator = new Random(new object().GetHashCode());
            int wylosowana;
            switch (OstatniTypStrzelania)
            {
                case LogikaStrzelania.Normalny:

                    wylosowana = generator.Next(listaMozliwychPolDoStrzalu.Count());
                    
                    OstatniePole = listaMozliwychPolDoStrzalu[wylosowana];

                    return OstatniePole;
                case LogikaStrzelania.Trafiony:
                    //Pobiera okolice po kwadracie
                    var pola =
                        listaMozliwychPolDoStrzalu.Where(
                            p =>
                                p.PorownajPolozenie(OstatniePole.X + 1, OstatniePole.Y) ||
                                p.PorownajPolozenie(OstatniePole.X - 1, OstatniePole.Y) ||
                                p.PorownajPolozenie(OstatniePole.X, OstatniePole.Y + 1) ||
                                p.PorownajPolozenie(OstatniePole.X, OstatniePole.Y - 1)).ToList();

                    wylosowana = generator.Next(pola.Count);
                    var pole = pola[wylosowana];
                    if (pole.TypPola == RodzajPola.Statek)
                    {
                        //Jesli pole nie jest poziomo, to znaczy, że jest pionowo
                        OstatniKierunek = pole.X - OstatniePole.X == 0 ? Kierunek.Dol : Kierunek.Prawo;
                        
                        OstatniePole = pole;
                    }
                    return pole;
                case LogikaStrzelania.ZKierunkiem:
                    var zbierzPolaDoStrzelania = new List<Pole>();
                    switch (OstatniKierunek)
                    {
                        case Kierunek.Dol:
                            //Idziemy w dol
                            int y = OstatniePole.Y + 1;
                            while (true)
                            {
                                //TODO: gowno kod
                                var znalezionePole =
                                    planszaGracza.ListaPol.SingleOrDefault(
                                        p => p.PorownajPolozenie(OstatniePole.X, y));
                                if (znalezionePole == null || znalezionePole.TypPola == RodzajPola.Pudlo) break;
                                if (znalezionePole.TypPola == RodzajPola.Statek ||
                                    znalezionePole.TypPola == RodzajPola.Puste)
                                {
                                    zbierzPolaDoStrzelania.Add(znalezionePole);
                                    break;
                                }
                                y++;
                            }
                            //Idziemy w gore
                            y = OstatniePole.Y - 1;
                            while (true)
                            {
                                var znalezionePole =
                                    planszaGracza.ListaPol.SingleOrDefault(
                                        p => p.PorownajPolozenie(OstatniePole.X, y));
                                if (znalezionePole == null || znalezionePole.TypPola == RodzajPola.Pudlo) break;
                                if (znalezionePole.TypPola == RodzajPola.Statek ||
                                    znalezionePole.TypPola == RodzajPola.Puste)
                                {
                                    zbierzPolaDoStrzelania.Add(znalezionePole);
                                    break;
                                }
                                y--;
                            }

                            //Losowanie Pola
                            wylosowana = generator.Next(zbierzPolaDoStrzelania.Count);
                            var losowePole = zbierzPolaDoStrzelania[wylosowana];
                            if (losowePole.TypPola == RodzajPola.Statek)
                            {
                                OstatniePole = losowePole;
                            }
                            return losowePole;
                        case Kierunek.Prawo:
                            //Idziemy w prawo
                            int x = OstatniePole.X + 1;
                            while (true)
                            {
                                var znalezionePole =
                                    planszaGracza.ListaPol.SingleOrDefault(
                                        p => p.PorownajPolozenie(x, OstatniePole.Y));
                                if (znalezionePole == null || znalezionePole.TypPola == RodzajPola.Pudlo) break;
                                if (znalezionePole.TypPola == RodzajPola.Statek ||
                                    znalezionePole.TypPola == RodzajPola.Puste)
                                {
                                    zbierzPolaDoStrzelania.Add(znalezionePole);
                                    break;
                                }
                                x++;
                            }
                            //Idziemy w lewo
                             x = OstatniePole.X - 1;
                            while (true)
                            {
                                var znalezionePole =
                                    planszaGracza.ListaPol.SingleOrDefault(
                                        p => p.PorownajPolozenie(x, OstatniePole.Y));
                                if (znalezionePole == null || znalezionePole.TypPola == RodzajPola.Pudlo) break;
                                if (znalezionePole.TypPola == RodzajPola.Statek ||
                                    znalezionePole.TypPola == RodzajPola.Puste)
                                {
                                    zbierzPolaDoStrzelania.Add(znalezionePole);
                                    break;
                                }
                                x--;
                            }

                            //Losowanie Pola
                            wylosowana = generator.Next(zbierzPolaDoStrzelania.Count);
                            var wylosowanePole = zbierzPolaDoStrzelania[wylosowana];
                            if (wylosowanePole.TypPola == RodzajPola.Statek)
                            {
                                OstatniePole = wylosowanePole;
                            }
                            return wylosowanePole;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Komputer()
        {
            OstatniTypStrzelania = LogikaStrzelania.Normalny;
        }

        private enum LogikaStrzelania
        {
            Normalny, Trafiony, ZKierunkiem
        }
    }
}
