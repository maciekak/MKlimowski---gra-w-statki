using System;
using System.Collections.Generic;
using System.Linq;

namespace MKlimowski___gra_w_statki
{
    public abstract class User
    {
        public const int QuantityOfShips = 4;
        public List<Ship> Ships { get; set; }
        public Board UsersBoard { get; set; }
        public ActionAfterShot LastAction { get; set; }
        public void PickShips()
        {
            var listOfPossibleFields = new List<Field>();
            Board.Copy(UsersBoard.ListOfFields, listOfPossibleFields);

            foreach (var ship in Ships)
            {
                do
                {
                    var generator = new Random(new object().GetHashCode());
                    int picked = generator.Next(listOfPossibleFields.Count);

                    ship.Direction = (Direction) generator.Next(2);
                    ship.X = listOfPossibleFields[picked].X;
                    ship.Y = listOfPossibleFields[picked].Y;
                }
                while (!Board.SetShips(listOfPossibleFields, ship));

                ship.IsSet = true;
                Board.RemoveShipAndSurround(listOfPossibleFields, ship);
            }
        }

        public ActionAfterShot Shot(int x, int y)
        {
            var field = UsersBoard.ListOfFields.First(p => p.CompareLocation(x, y));

            switch(field.TypeOfField)
            {
                case KindOfField.Empty:
                    field.TypeOfField = KindOfField.Miss;
                    return ActionAfterShot.Miss;
                case KindOfField.Miss:
                    return ActionAfterShot.Error;
                case KindOfField.Hit:
                    return ActionAfterShot.Error;
                case KindOfField.Ship:
                    field.TypeOfField = KindOfField.Hit;
                    return Sink(field) ? ActionAfterShot.Sinked : ActionAfterShot.Hit;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool Sink(Field field)
        {
            var collectHitFields = new List<Field> {field};
            
            // Sprawdzanie czy na lewo jest statek
            var lastX = field.X - 1;
            while (true)
            {
                var neighbouringField =
                    UsersBoard.ListOfFields.FirstOrDefault(p => p.CompareLocation(lastX, field.Y));

                if (neighbouringField == null || neighbouringField.TypeOfField == KindOfField.Miss ||
                    neighbouringField.TypeOfField == KindOfField.Empty) break;
                //Jesli jest nietrafione pole ze statkiem to mamy pewnosc, ze statek nie jest zatopiony - analogicznie w kazdym
                if (neighbouringField.TypeOfField == KindOfField.Ship) return false;
                collectHitFields.Add(neighbouringField);
                lastX--;
            }

            // Sprawdzanie czy na prawo jest statek
            lastX = field.X + 1;
            while (true)
            {
                var neighbouringField =
                    UsersBoard.ListOfFields.FirstOrDefault(p => p.CompareLocation(lastX, field.Y));
                if (neighbouringField == null || neighbouringField.TypeOfField == KindOfField.Miss ||
                    neighbouringField.TypeOfField == KindOfField.Empty) break;
                if (neighbouringField.TypeOfField == KindOfField.Ship) return false;
                collectHitFields.Add(neighbouringField);
                lastX++;
            }

            // Sprawdzanie czy w gore jest statek
            int lastY = field.Y - 1;
            while (true)
            {
                var neighbouringField =
                    UsersBoard.ListOfFields.FirstOrDefault(p => p.CompareLocation(field.X, lastY));
                if (neighbouringField == null || neighbouringField.TypeOfField == KindOfField.Miss ||
                    neighbouringField.TypeOfField == KindOfField.Empty) break;
                if (neighbouringField.TypeOfField == KindOfField.Ship) return false;
                collectHitFields.Add(neighbouringField);
                lastY--;
            }

            // Sprawdzanie czy w dol jest statek
            lastY = field.Y + 1;
            while (true)
            {
                var neighbouringField =
                    UsersBoard.ListOfFields.FirstOrDefault(p => p.CompareLocation(field.X, lastY));
                if (neighbouringField == null || neighbouringField.TypeOfField == KindOfField.Miss ||
                    neighbouringField.TypeOfField == KindOfField.Empty) break;
                if (neighbouringField.TypeOfField == KindOfField.Ship) return false;
                collectHitFields.Add(neighbouringField);
                lastY++;
            }

            var ship = Ships.First(s => collectHitFields.Exists(p => s.X == p.X && s.Y == p.Y));
            ship.IsSinked = true;

            var surround = UsersBoard.FindSurround(collectHitFields);

            //Ustawia wszystkie sasiednie pola statku jako pudla
            surround.ForEach(p => p.TypeOfField = KindOfField.Miss);
            return true;
        }

        public bool CheckIsEnd()
        {
            //Jesli nie ma zadnego pola ze statkiem to zwraca prawde
            return !UsersBoard.ListOfFields.Exists(p => p.TypeOfField == KindOfField.Ship);
        }

        protected User()
        {
            UsersBoard = new Board();
            UsersBoard.SetAtStart();
            Ships = new List<Ship>();
            for (int i = QuantityOfShips; i >= 1; i--)
            {
                for (int j = QuantityOfShips; j >= i; j--)
                {
                    Ships.Add(new Ship(i));
                }
            }
        }
    }

    public enum ActionAfterShot
    {
        Hit, Miss, Sinked, Error
    }
}
