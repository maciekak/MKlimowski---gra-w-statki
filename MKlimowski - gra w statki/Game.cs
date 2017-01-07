namespace MKlimowski___gra_w_statki
{
    public class Game
    {
        public Player Player { get; set; }
        public Computer Pc { get; set; }

        public Game()
        {
            Player = new Player();
            Pc = new Computer();
        }

        public bool ComputerMove()
        {
            var pickedField = Pc.PickField(Player.UsersBoard, Player.Ships);
            Pc.LastAction = Player.Shot(pickedField.X, pickedField.Y);
            Pc.SetLastTypeOfShooting(Pc.LastAction);
            return Player.CheckIsEnd();
        }
    }
}
