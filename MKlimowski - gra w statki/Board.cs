using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace MKlimowski___gra_w_statki
{
    public class Board
    {
        public List<Field> ListOfFields { get; }

        public Board()
        {
            ListOfFields = new List<Field>();
        }

        public void SetAtStart()
        {
            for (int y = 0; y < MainWindow.Measurement; y++)
            {
                for (int x = 0; x < MainWindow.Measurement; x++)
                {
                    ListOfFields.Add(new Field(x, y));
                }
            }
        }

        public void Reset()
        {
            ListOfFields.ForEach(p => p.TypeOfField = KindOfField.Empty);
        }

        public static void Copy(List<Field> fromList, List<Field> toList)
        {
            fromList.ForEach(toList.Add);
        }

        public static bool RemoveField(int x, int y, List<Field> listOfFields )
        {
            var itemToRemove = listOfFields.Single(p => p.CompareLocation(x, y));
            return listOfFields.Remove(itemToRemove);
        }

        public List<Field> FindSurround(List<Field> fields)
        {
            var surround = fields.SelectMany(p => ListOfFields.Where(a => a.CheckIsInSurround(p.X, p.Y))).ToList();

            surround = surround.Distinct().ToList();
            surround.RemoveAll(fields.Contains);

            return surround;
        }
        
        public static bool RemoveShipAndSurround(List<Field> fieldsOfBoards, Ship ship)
        {
            //TODO: gownokod przerobic z wykorzystaniem powyzszej metody
            int lenghtOfOriginalBoard = fieldsOfBoards.Count;
            List<Field> fields;
            switch (ship.Direction)
            {
                case Direction.Down:
                    fields =
                        fieldsOfBoards.Where( p => p.X >= ship.X - 1 && p.X <= ship.X + 1 && p.Y >= ship.Y - 1 && p.Y < ship.Y + ship.Length + 1).ToList();
                    break;
                case Direction.Right:
                    fields =
                        fieldsOfBoards.Where(
                            p =>
                                p.Y >= ship.Y - 1 && p.Y <= ship.Y + 1 && p.X >= ship.X - 1 &&
                                p.X < ship.X + ship.Length + 1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            fields.ForEach(p => fieldsOfBoards.Remove(p));

            return fieldsOfBoards.Count + fields.Count == lenghtOfOriginalBoard;
        }

        public static bool SetShips(List<Field> baseFields, Ship ship)
        {
            //TODO: no kurwa popraw, bo chujowo
            List<Field> fields;
            switch (ship.Direction)
            {
                case Direction.Down:
                    fields = baseFields.Where(p => p.X == ship.X && p.Y >= ship.Y && p.Y < ship.Y + ship.Length && p.TypeOfField == KindOfField.Empty).ToList();
                    break;
                case Direction.Right:
                    fields = baseFields.Where(p => p.Y == ship.Y && p.X >= ship.X && p.X < ship.X + ship.Length && p.TypeOfField == KindOfField.Empty).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ship.Direction), ship.Direction, null);
            }
            if (fields.Count != ship.Length) return false;

            fields.ForEach(p => p.TypeOfField = KindOfField.Ship);
            return true;
        }

        private void SetPriorities(Field field, int length)
        {
            if (field.TypeOfField == KindOfField.Hit || field.TypeOfField == KindOfField.Miss)
            {
                field.Priority = 0;
                return;
            }

            int quantityOfHorizontallyFields = 1;
            //Dodajemy kolejne pola w prawo
            for(int x = field.X + 1; x < field.X + length; x++)
            {
                var foundFields =
                    ListOfFields.SingleOrDefault(
                        p => p.CompareLocation(x, field.Y));

                if (foundFields == null || foundFields.TypeOfField == KindOfField.Hit || foundFields.TypeOfField == KindOfField.Miss)
                    break;

                quantityOfHorizontallyFields++;
            }
            //Dodajemy kolejne pola w lewo
            for (int x = field.X - 1; x > field.X - length; x--)
            {
                var foundFields =
                    ListOfFields.SingleOrDefault(
                        p => p.CompareLocation(x, field.Y));

                if (foundFields == null || foundFields.TypeOfField == KindOfField.Hit || foundFields.TypeOfField == KindOfField.Miss)
                    break;

                quantityOfHorizontallyFields++;
            }

            int quantityOfVerticallyFields = 1;
            //Dodajemy kolejne pola w dol
            for (int y = field.Y + 1; y < field.Y + length; y++)
            {
                var foundFields =
                    ListOfFields.SingleOrDefault(
                        p => p.CompareLocation(field.X, y));

                if (foundFields == null || foundFields.TypeOfField == KindOfField.Hit || foundFields.TypeOfField == KindOfField.Miss)
                    break;

                quantityOfVerticallyFields++;
            }
            //Dodajemy kolejne pola w gore
            for (int y = field.Y - 1; y > field.Y - length; y--)
            {
                var foundFields =
                    ListOfFields.SingleOrDefault(
                        p => p.CompareLocation(field.X, y));

                if (foundFields == null || foundFields.TypeOfField == KindOfField.Hit || foundFields.TypeOfField == KindOfField.Miss)
                    break;

                quantityOfVerticallyFields++;
            }

            int quantityOfPossibilitiesVertically = quantityOfVerticallyFields - length + 1;
            int quantityOfPossibilitiesHorizontally = quantityOfHorizontallyFields - length + 1;

            int priority = quantityOfPossibilitiesHorizontally > 0 ? quantityOfPossibilitiesHorizontally : 0;
            priority += quantityOfPossibilitiesVertically > 0 ? quantityOfPossibilitiesVertically : 0;

            field.Priority = priority;
        }

        public List<Field> FindPriorityListOfFieldsForLongestShip(List<Ship> ships)
        {
            int length;
            for (length = 4; length > 0; length--)
            {
                int quantity = ships.Count(s => s.IsSinked == false && s.Length == length);
                if (quantity > 0)
                    break;
            }

            if (length <= 0)
                throw new ValueUnavailableException("Brak niezatopionych statkow");

            if (length == 1)
            {
                var fields = ListOfFields.Where(p => p.TypeOfField == KindOfField.Empty || p.TypeOfField == KindOfField.Ship).ToList();
                fields.ForEach(p => p.Priority = 1);
                return fields;
            }

            var fieldsWithPriorities = new List<Field>();
            foreach (var pole in ListOfFields)
            {
                SetPriorities(pole, length);
                if(pole.Priority > 0)
                    fieldsWithPriorities.Add(pole);
            }

            return fieldsWithPriorities;
        }
    }
    public enum Direction
    {
        Down, Right
    }
}
