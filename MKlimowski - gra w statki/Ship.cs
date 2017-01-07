namespace MKlimowski___gra_w_statki
{
    public class Ship : ILocation
    {
        public int Length { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Direction { get; set; }
        public bool IsSet { get; set; }
        public bool IsSinked { get; set; }

        public Ship(int length)
        {
            Length = length;
            IsSet = false;
            IsSinked = false;
        }
    }
}
