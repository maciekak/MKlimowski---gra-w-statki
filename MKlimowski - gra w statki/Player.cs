using System.Linq;

namespace MKlimowski___gra_w_statki
{
    public class Player : User
    {
        public new void PickShips()
        {
            UsersBoard.Reset();
            base.PickShips();

        }

        public void Reset()
        {
            //Reset Statkow
            Ships = Ships.Select(s => new Ship(s.Length)).ToList();

            //Reset Planszy
            UsersBoard.Reset();
        }

        public bool CheckIfSet()
        {
            return !Ships.Exists(x => x.IsSet == false);
        }
    }
}
