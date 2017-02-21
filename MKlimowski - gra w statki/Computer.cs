using System;
using System.Collections.Generic;
using System.Linq;

namespace MKlimowski___gra_w_statki
{
    public class Computer : User
    {
        private Field LastField { get; set; }
        private ShootingLogic LastTypeOfShooting { get; set; }
        private Direction LastDirection { get; set; }
        
        public void SetLastTypeOfShooting(ActionAfterShot action)
        {
            switch (action)
            {
                case ActionAfterShot.Hit:
                    LastTypeOfShooting = LastTypeOfShooting == ShootingLogic.Normal
                        ? ShootingLogic.Hit
                        : ShootingLogic.HitWithDirection;
                    break;
                case ActionAfterShot.Miss:
                    //Nic nie robimy, bo jesli bylo 'trafiony' to dalej strzela w ten statek
                    //jesli byl 'normalny' to dalej bedzie celowal po calej planszy
                    break;
                case ActionAfterShot.Sinked:
                    LastTypeOfShooting = ShootingLogic.Normal;
                    break;
                case ActionAfterShot.Error:
                    LastTypeOfShooting = ShootingLogic.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        public Field PickField(Board playerBoard, List<Ship> ships)
        {
            //Znajdywanie pola gdzie ma strzelic komputer
            //Poczatkowo rozwiazanie bylo po prostu na liscie pol, jesli pole bylo trafione
            //to wypadalo z listy, za kazdym razem bylo losowanie z tej listy, wiec zawsze
            //bylo losowanie tylko z mozliwych pol + w miare standardowa logika jesli juz trafil
            //Ale wpadlem na pomysl, zeby komputer szukal pol gdzie moga stac po kolei najdluzsze statki
            //podobnie jak czlowiek, pierwsze szuka czworki pozniej trojki itp itd
            //zeby to zrobic nadalem polom priorytety, ktore oznaczaja ilosc mozliwosci na ktore moga
            //stac statki (np jesli szukamy czteromasztowca na polu (0,0) priorytet jest 2,
            //bo statek moze stac w dol od tego pola i na prawo
            //a np na polu (1,0) priorytet wynosi 3 bo statek moze stac w dol i na dwie mozliwosci w prawo
            //(zaczyna sie na (0,0) i w prawo oraz zaczyna sie na (1,0) i w prawo))
            //Pozniej odbywa sie losowanie priorytetowe, o tym wiecej juz w odpowiedniej funkcji :)
            var generator = new Random(new object().GetHashCode());
            switch (LastTypeOfShooting)
            {
                case ShootingLogic.Normal:

                    var priorityListOfFieldsForLongestShip =
                        playerBoard.FindPriorityListOfFieldsForLongestShip(ships);

                    LastField = PickByPriority(priorityListOfFieldsForLongestShip);
                    return LastField;

                case ShootingLogic.Hit:
                    //Jesli trafiono w statek to komputer znajduje 4 okoliczne pola i losuje z tych 4 kolejne to strzalu
                    var listOfPossibleFieldsToShot =
                        playerBoard.ListOfFields.Where(p => p.TypeOfField == KindOfField.Empty || p.TypeOfField == KindOfField.Ship).ToList();
                    var fields =
                        listOfPossibleFieldsToShot.Where(
                            p =>
                                p.CompareLocation(LastField.X + 1, LastField.Y) ||
                                p.CompareLocation(LastField.X - 1, LastField.Y) ||
                                p.CompareLocation(LastField.X, LastField.Y + 1) ||
                                p.CompareLocation(LastField.X, LastField.Y - 1)).ToList();

                    var field = GetRandomField(fields, generator);

                    //Jesli wczesniej juz trafilismy w statek, czyli to jest drugi strzal w statek, to juz mozna wyliczyc kierunek w jakim stoi
                    if (field.TypeOfField == KindOfField.Ship)
                    {
                        //Jesli pole nie jest poziomo, to znaczy, że jest pionowo
                        LastDirection = field.X - LastField.X == 0 ? Direction.Down : Direction.Right;
                        
                        LastField = field;
                    }
                    return field;
                case ShootingLogic.HitWithDirection:
                    //Kiedy znamy kierunek (np w prawo) to idziemy maksymalnie w prawo na nietrafione pole i pobieramy je
                    //tak samo maksymalnie w lewo i pozniej losujemy z tych dwoch pol. Pola nie pobieramy jesli juz strzelilismy tam
                    //jesli bylo trafione w statek to idziemy dalej w odpowiednim kierunku, jesli bylo pudlo to nie pobieramy z tej strony pola
                    //Na koniec losujemy pole ze znalezionych
                    var collectFieldsToShot = new List<Field>();
                    switch (LastDirection)
                    {
                        case Direction.Down:
                            //Idziemy w dol
                            int y = LastField.Y + 1;
                            while (true)
                            {
                                if (AddPossibleField(playerBoard, collectFieldsToShot, LastField.X, y))
                                    break;
                                y++;
                            }
                            //Idziemy w gore
                            y = LastField.Y - 1;
                            while (true)
                            {
                                if (AddPossibleField(playerBoard, collectFieldsToShot, LastField.X, y))
                                    break;
                                y--;
                            }

                            //Losowanie Pola
                            var pickedField = GetRandomField(collectFieldsToShot, generator);
                            if (pickedField.TypeOfField == KindOfField.Ship)
                            {
                                LastField = pickedField;
                            }
                            return pickedField;
                        case Direction.Right:
                            //Idziemy w prawo
                            int x = LastField.X + 1;
                            while (true)
                            {
                                if (AddPossibleField(playerBoard, collectFieldsToShot, x, LastField.Y))
                                    break;
                                x++;
                            }
                            //Idziemy w lewo
                             x = LastField.X - 1;
                            while (true)
                            {
                                if (AddPossibleField(playerBoard, collectFieldsToShot, x, LastField.Y))
                                    break;
                                x--;
                            }

                            //Losowanie Pola
                            var fieldPicked = GetRandomField(collectFieldsToShot, generator);
                            if (fieldPicked.TypeOfField == KindOfField.Ship)
                            {
                                LastField = fieldPicked;
                            }
                            return fieldPicked;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //Zwraca false jesli pole jest trafionym statkiem, true jesli nie
        private static bool AddPossibleField(Board playerBoard, List<Field> collectFieldsToShot, int x, int y)
        {
            var foundField =
                playerBoard.ListOfFields.SingleOrDefault(
                    p => p.CompareLocation(x, y));

            if (foundField == null || foundField.TypeOfField == KindOfField.Miss)
                return true;

            if (foundField.TypeOfField == KindOfField.Hit)
                return false;

            collectFieldsToShot.Add(foundField);
            return true;
        }

        private static Field GetRandomField(List<Field> collectFieldsToShot, Random generator)
        {
            int picked = generator.Next(collectFieldsToShot.Count);
            return collectFieldsToShot[picked];
        }

        private static Field PickByPriority(List<Field> priorityList)
        {
            //Losowanie priorytetowe
            //Tworzy sie nowa liste z polami w zaleznosci od priorytetu, to znaczy dane pole w tej liscie
            //powtarza sie tyle razy jaki ma priorytet
            //Np jesli na wejsciu mamy 'A' o priorytecie 3 i 'B' o priorytecie 2 i 'C' o priorytecie 1
            //to po stworzeniu listy bedziemy mieli 'A','A','A','B','B','C' i z tej listy juz normalne losowanie
            var flatList = new List<Field>();

            foreach (var field in priorityList)
            {
                flatList.AddRange(Enumerable.Repeat(field, field.Priority));
            }

            var generator = new Random(new object().GetHashCode());
            int picked = generator.Next(flatList.Count);
            return flatList[picked];
        }

        public Computer()
        {
            LastTypeOfShooting = ShootingLogic.Normal;
        }

        private enum ShootingLogic
        {
            Normal, Hit, HitWithDirection
        }

    }
}
