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
            var listOfPossibleFieldsToShot =
                playerBoard.ListOfFields.Where(p => p.TypeOfField == KindOfField.Empty || p.TypeOfField == KindOfField.Ship).ToList();
            var generator = new Random(new object().GetHashCode());
            int picked;
            switch (LastTypeOfShooting)
            {
                case ShootingLogic.Normal:

                    var priorityListOfFieldsForLongestShip =
                        playerBoard.FindPriorityListOfFieldsForLongestShip(ships);

                    LastField = PickByPriority(priorityListOfFieldsForLongestShip);
                    return LastField;

                case ShootingLogic.Hit:
                    //Pobiera okolice po kwadracie
                    var fields =
                        listOfPossibleFieldsToShot.Where(
                            p =>
                                p.CompareLocation(LastField.X + 1, LastField.Y) ||
                                p.CompareLocation(LastField.X - 1, LastField.Y) ||
                                p.CompareLocation(LastField.X, LastField.Y + 1) ||
                                p.CompareLocation(LastField.X, LastField.Y - 1)).ToList();

                    picked = generator.Next(fields.Count);
                    var field = fields[picked];
                    if (field.TypeOfField == KindOfField.Ship)
                    {
                        //Jesli pole nie jest poziomo, to znaczy, że jest pionowo
                        LastDirection = field.X - LastField.X == 0 ? Direction.Down : Direction.Right;
                        
                        LastField = field;
                    }
                    return field;
                case ShootingLogic.HitWithDirection:
                    var collectFieldsToShot = new List<Field>();
                    switch (LastDirection)
                    {
                        case Direction.Down:
                            //Idziemy w dol
                            int y = LastField.Y + 1;
                            while (true)
                            {
                                //TODO: gowno kod
                                var foundField =
                                    playerBoard.ListOfFields.SingleOrDefault(
                                        p => p.CompareLocation(LastField.X, y));
                                if (foundField == null || foundField.TypeOfField == KindOfField.Miss) break;
                                if (foundField.TypeOfField == KindOfField.Ship ||
                                    foundField.TypeOfField == KindOfField.Empty)
                                {
                                    collectFieldsToShot.Add(foundField);
                                    break;
                                }
                                y++;
                            }
                            //Idziemy w gore
                            y = LastField.Y - 1;
                            while (true)
                            {
                                var foundField =
                                    playerBoard.ListOfFields.SingleOrDefault(
                                        p => p.CompareLocation(LastField.X, y));
                                if (foundField == null || foundField.TypeOfField == KindOfField.Miss) break;
                                if (foundField.TypeOfField == KindOfField.Ship ||
                                    foundField.TypeOfField == KindOfField.Empty)
                                {
                                    collectFieldsToShot.Add(foundField);
                                    break;
                                }
                                y--;
                            }

                            //Losowanie Pola
                            picked = generator.Next(collectFieldsToShot.Count);
                            var pickedField = collectFieldsToShot[picked];
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
                                var foundField =
                                    playerBoard.ListOfFields.SingleOrDefault(
                                        p => p.CompareLocation(x, LastField.Y));
                                if (foundField == null || foundField.TypeOfField == KindOfField.Miss) break;
                                if (foundField.TypeOfField == KindOfField.Ship ||
                                    foundField.TypeOfField == KindOfField.Empty)
                                {
                                    collectFieldsToShot.Add(foundField);
                                    break;
                                }
                                x++;
                            }
                            //Idziemy w lewo
                             x = LastField.X - 1;
                            while (true)
                            {
                                var foundField =
                                    playerBoard.ListOfFields.SingleOrDefault(
                                        p => p.CompareLocation(x, LastField.Y));
                                if (foundField == null || foundField.TypeOfField == KindOfField.Miss) break;
                                if (foundField.TypeOfField == KindOfField.Ship ||
                                    foundField.TypeOfField == KindOfField.Empty)
                                {
                                    collectFieldsToShot.Add(foundField);
                                    break;
                                }
                                x--;
                            }

                            //Losowanie Pola
                            picked = generator.Next(collectFieldsToShot.Count);
                            var fieldPicked = collectFieldsToShot[picked];
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

        private static Field PickByPriority(List<Field> priorityList)
        {
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
