using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKlimowski___gra_w_statki
{
    public class GrabbingShipsData : ILocation
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsGrabbed { get; set; }
        public int Length { get; set; }
        public Direction Direction { get; set; }
        public List<Field> FieldsOfPotentialShip { get; set; }
        public List<Field> AvailableFields { get; set; }
        public int QuantityOfUnsituated { get; set; }
        public bool IsPossibleToPlace { get; set; }

        public GrabbingShipsData()
        {
            FieldsOfPotentialShip = null;
            AvailableFields = null;
            IsGrabbed = false;
            IsPossibleToPlace = false;
            Length = 0;
            Direction = Direction.Down;
        }

    }
}
