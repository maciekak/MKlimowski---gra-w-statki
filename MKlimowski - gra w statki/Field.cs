namespace MKlimowski___gra_w_statki
{
    public class Field : ILocation
    {
        public int X { get; }
        public int Y { get; }
        public KindOfField TypeOfField { get; set; }
        public int Priority { get; set; }
        public Field(int x, int y, KindOfField typeOfField = KindOfField.Empty)
        {
            X = x;
            Y = y;
            TypeOfField = typeOfField;
            Priority = 0;
        }

        public bool CompareLocation(int x, int y)
        {
            return X == x && Y == y;
        }

        public bool CheckIsInSurround(int x, int y)
        {
            return X >= x - 1 && X <= x + 1 && Y >= y - 1 && Y <= y + 1;
        }
    }
}
