using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MKlimowski___gra_w_statki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    /*Przepraszam za lekki bajzel w kodzie, ale moze to glupie tak sie tlumaczyc
     * ale nie zdazylem zrefactoryzowac kodu. Duzo jest powtarzajacego sie kodu,
     * ktory mozna wydzielic do osobnych funkcji czy klas. Klasy tez sa za duze, 
     * jak na przyklad ta.
     * Ogolnie mozna bylo ta gre napisac znacznie krocej, ale staralem sie robic
     * tak, zeby inne pliki nie zalezaly od "Widoku" czyli tego wlasnie pliku,
     * a widok natomiast zalezal tylko od klasy Game, ale nie udalo mi sie tego
     * ostatniego zrobic :)
     * Caly kod jest rowniez na moim githubie: https://github.com/maciekak
     * Chcialem jeszcze dodac, ze nie jestem mistrzem w grafice ani temu, zeby 
     * to ladnie wygladalo.
     *
    */
    public partial class MainWindow : Window
    {
        public const int Measurement = 10;
        private Game Game { get; }
        private Dictionary<KindOfField, Uri> UrisImagesToGame { get; set; }
        private Dictionary<bool, Uri> UriPossibilitiesToPlaceShip { get; set; }
        private State GameState { get; set; }
        private GrabbingShipsData LastGrap { get; }

        private enum State
        {
            Start, Preordering, PlayerMove, ComputerMove, End
        }
        
        public MainWindow()
        {
            InitializeComponent();

            Instructions.Text = "Instrukcje:\nNaciśnij START.";
            
            double columnWidth = MainGrid.ColumnDefinitions[0].Width.Value / Measurement;
            double rowHeight = (MainGrid.RowDefinitions[1].Height.Value) / Measurement;

            SetImagesOfFields();

            CreateBoard(ComputerBoard, columnWidth, rowHeight);
            CreateBoard(PlayerBoard, columnWidth, rowHeight);
            
            PlayerOneDecker.Source = ComputerOneDecker.Source = OneDecker.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Statek.png"));
            PlayerTwoDecker.Source = ComputerTwoDecker.Source = TwoDecker.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\2xStatek.png"));
            PlayerThreeDecker.Source = ComputerThreeDecker.Source = ThreeDecker.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\3xStatek.png"));
            PlayerFourDecker.Source = ComputerFourDecker.Source = FourDecker.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\4xStatek.png"));

            LastGrap = new GrabbingShipsData();
            Game = new Game();
            GameState = State.Start;
        }

        private void SetImagesOfFields()
        {
            UrisImagesToGame = new Dictionary<KindOfField, Uri>()
            {
                {KindOfField.Empty, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png")},
                {KindOfField.Miss, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Pudlo.png")},
                {KindOfField.Hit, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Trafiony.png")},
                {KindOfField.Ship, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Statek.png")}
            };

            UriPossibilitiesToPlaceShip = new Dictionary<bool, Uri>()
            {
                {true, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\PropozycjaStatek.png")},
                {false, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\NiedozwolonyStatek.png")}
            };
        }

        public static void SetCursorOfBoard(Grid grid, Cursor cursor)
        {
            var images = grid.Children.OfType<Image>().ToList();
            images.ForEach(o => o.Cursor = cursor);
        }

        private void CreateBoard(Grid board, double width, double height)
        {
            //Tworzenie siatki pol
            for (int i = 0; i < Measurement; i++)
            {
                board.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(width)
                });
                board.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(height)
                });
            }

            //Wypelnianie nietrafionymi polami
            for (int x = 0; x < Measurement; x++)
            {
                for (int y = 0; y < Measurement; y++)
                {
                    var field = new Image
                    {
                        Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png"))
                    };
                    field.SetValue(Grid.RowProperty, x);
                    field.SetValue(Grid.ColumnProperty, y);
                    board.Children.Add(field);
                }
            }
            
        }

        public void Show(bool isSetHandCursor = true)
        {
            var cursor = isSetHandCursor ? Cursors.Hand : Cursors.Arrow;

            //Rysuj plansze Gracza
            foreach (var field in Game.Player.UsersBoard.ListOfFields)
            {
                var element = PlayerBoard.Children.OfType<Image>().First(i => Grid.GetRow(i) == field.X && Grid.GetColumn(i) == field.Y);
                element.Source = new BitmapImage(UrisImagesToGame[field.TypeOfField]);
            }

            //Rysuj plansze komputera
            foreach (var field in Game.Pc.UsersBoard.ListOfFields)
            {
                var element = ComputerBoard.Children.OfType<Image>().First(i => Grid.GetRow(i) == field.X && Grid.GetColumn(i) == field.Y);
                //Jesli polem jest statek, to ustaw jako puste, zeby gracz nie widzial gdzie statki ma komputer
                element.Source = field.TypeOfField == KindOfField.Ship ? new BitmapImage(UrisImagesToGame[KindOfField.Empty]) : new BitmapImage(UrisImagesToGame[field.TypeOfField]);
                element.Cursor = field.TypeOfField == KindOfField.Miss || field.TypeOfField == KindOfField.Hit
                    ? Cursors.Arrow
                    : cursor;
            }
        }

        public static void AssignMouseButtonEventToBoard(Grid grid, MouseButtonEventHandler mouseButtonEventHandler, RoutedEvent routedEvent)
        {
            var listOfImages = grid.Children.OfType<Image>().ToList();
            listOfImages.ForEach(i => i.AddHandler(routedEvent, mouseButtonEventHandler));
        }

        public static void AssignMouseEventToBoard(Grid grid, MouseEventHandler mouseEventHandler, RoutedEvent routedEvent)
        {
            var listOfImages = grid.Children.OfType<Image>().ToList();
            listOfImages.ForEach(i => i.AddHandler(routedEvent, mouseEventHandler));
        }

        //Trzy ponizsze metody wygladaja strasznie :)
        public void ShowInfromationToSettingShips()
        {
            OneDecker.IsEnabled = true;
            OneDecker.Visibility = Visibility.Visible;
            QuantityOfOneDecker.Visibility = Visibility.Visible;
            BorderOfOneDecker.Visibility = Visibility.Visible;

            TwoDecker.IsEnabled = true;
            TwoDecker.Visibility = Visibility.Visible;
            QuantityOfTwoDecker.Visibility = Visibility.Visible;
            BorderOfTwoDecker.Visibility = Visibility.Visible;

            ThreeDecker.IsEnabled = true;
            ThreeDecker.Visibility = Visibility.Visible;
            QuantityOfThreeDecker.Visibility = Visibility.Visible;
            BorderOfThreeDecker.Visibility = Visibility.Visible;

            FourDecker.IsEnabled = true;
            FourDecker.Visibility = Visibility.Visible;
            QuantityOfFourDecker.Visibility = Visibility.Visible;
            BorderOfFourDecker.Visibility = Visibility.Visible;

            Pick.Visibility = Visibility.Visible;
            Reset.Visibility = Visibility.Visible;
            Turn.Visibility = Visibility.Visible;

            ActionButton.Content = "Rozstawione";
            ActionButton.IsEnabled = false;
            Informations.Text = "Statki:";
            Instructions.Text = "Intrukcje:\nWybierz statek, a następnie wybierz mu miejsce na planszy.";

            SetCursorOfBoard(InformationGrid, Cursors.Hand);

            AssignMouseButtonEventToBoard(InformationGrid, Image_Grabbing, MouseDownEvent);
            AssignMouseButtonEventToBoard(PlayerBoard, Image_DroppingShips, MouseDownEvent);

            AssignMouseEventToBoard(PlayerBoard, Image_EnteringCursorOnField, MouseEnterEvent);
            AssignMouseEventToBoard(PlayerBoard, Image_LeavingCursorFromField, MouseLeaveEvent);
        }

        public void HideInformationToSettingsShips()
        {
            OneDecker.IsEnabled = false;
            OneDecker.Visibility = Visibility.Hidden;
            QuantityOfOneDecker.Visibility = Visibility.Hidden;
            BorderOfOneDecker.Visibility = Visibility.Hidden;

            TwoDecker.IsEnabled = false;
            TwoDecker.Visibility = Visibility.Hidden;
            QuantityOfTwoDecker.Visibility = Visibility.Hidden;
            BorderOfTwoDecker.Visibility = Visibility.Hidden;

            ThreeDecker.IsEnabled = false;
            ThreeDecker.Visibility = Visibility.Hidden;
            QuantityOfThreeDecker.Visibility = Visibility.Hidden;
            BorderOfThreeDecker.Visibility = Visibility.Hidden;

            FourDecker.IsEnabled = false;
            FourDecker.Visibility = Visibility.Hidden;
            QuantityOfFourDecker.Visibility = Visibility.Hidden;
            BorderOfFourDecker.Visibility = Visibility.Hidden;

            Pick.Visibility = Visibility.Hidden;
            Reset.Visibility = Visibility.Hidden;
            Turn.Visibility = Visibility.Hidden;

            SetCursorOfBoard(InformationGrid, Cursors.Arrow);
            SetCursorOfBoard(PlayerBoard, Cursors.Arrow);
            Informations.Text = "";
            
            ShowStatistics();
        }

        public void ShowStatistics()
        {
            PlayerOneDecker.Visibility = Visibility.Visible;
            PlayerTwoDecker.Visibility = Visibility.Visible;
            PlayerThreeDecker.Visibility = Visibility.Visible;
            PlayerFourDecker.Visibility = Visibility.Visible;

            ComputerOneDecker.Visibility = Visibility.Visible;
            ComputerTwoDecker.Visibility = Visibility.Visible;
            ComputerThreeDecker.Visibility = Visibility.Visible;
            ComputerFourDecker.Visibility = Visibility.Visible;

            PlayerQuantityOfOneDecker.Visibility = Visibility.Visible;
            PlayerQuantityOfTwoDecker.Visibility = Visibility.Visible;
            PlayerQuantityOfThreeDecker.Visibility = Visibility.Visible;
            PlayerQuantityOfFourDecker.Visibility = Visibility.Visible;

            ComputerQuantityOfOneDecker.Visibility = Visibility.Visible;
            ComputerQuantityOfTwoDecker.Visibility = Visibility.Visible;
            ComputerQuantityOfThreeDecker.Visibility = Visibility.Visible;
            ComputerQuantityOfFourDecker.Visibility = Visibility.Visible;

            PlayerStatistics.Visibility = Visibility.Visible;
            ComputerStatistics.Visibility = Visibility.Visible;
        }

        public void Image_ShootingInComputerFields(object sender, RoutedEventArgs e)
        {
            if (GameState != State.PlayerMove)
                return;

            int y = Grid.GetColumn((Image) sender);
            int x = Grid.GetRow((Image) sender);

            var actionAfterShot = Game.Pc.Shot(x, y);

            switch(actionAfterShot)
            {
                case ActionAfterShot.Hit:
                    Informations.Text = "Trafiony!";
                    GameState = State.PlayerMove;
                    break;

                case ActionAfterShot.Miss:
                    ActionButton.Content = "Ruch Komputera";
                    Informations.Text = "Pudło!";
                    Show(false);

                    GameState = State.ComputerMove;
                    ComputerMove();
                    break;

                case ActionAfterShot.Sinked:
                    Informations.Text = "Trafiony Zatopiony!";
                    UpdatePlayerStatistics();
                    GameState = State.PlayerMove;

                    //Jesli Gracz wygral
                    if (Game.Pc.CheckIsEnd())
                        EndGame(Game.Player);
                    break;
                case ActionAfterShot.Error:
                    Informations.Text = "Tu już strzelałeś";
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Show(GameState == State.PlayerMove);
        }

        public void Image_Grabbing(object sender, RoutedEventArgs e)
        {
            //Sprawdzic czy mozna chwycic
            if (GameState != State.Preordering || LastGrap.IsGrabbed)
                return;

            int length = FindShipLength(sender as Image);
            int quantity = Game.Player.Ships.Count(s => s.Length == length && s.IsSet == false);
            if (quantity <= 0)
                return;

            LastGrap.Length = length;
            LastGrap.IsGrabbed = true;
            LastGrap.Direction = Direction.Down;
            LastGrap.QuantityOfUnsituated = quantity;
            
            SetCursorOfBoard(InformationGrid, Cursors.Arrow);
        }

        private static int FindShipLength(Image obrazek)
        {
            int row = Grid.GetRow(obrazek);
            if(row > 4 || row < 1)
                throw new ArgumentException("Niepoprawny wiersz");

            return 5 - row; //bo w pierwszym wierszu mamy czteromasztowca, w drugim troj itp itd

        }

        public void Image_DroppingShips(object sender, RoutedEventArgs e)
        {
            //Sprawdzic czy jestesmy w odpowiednim stanie
            if (GameState != State.Preordering || !LastGrap.IsGrabbed || !LastGrap.IsPossibleToPlace)
                return;

            //Zmienic kolor statku
            foreach (var field in LastGrap.AvailableFields)
            {
                var element = PlayerBoard.Children.OfType<Image>().First(i => Grid.GetRow(i) == field.X && Grid.GetColumn(i) == field.Y);
                element.Source = new BitmapImage(UrisImagesToGame[KindOfField.Ship]);
            }

            //Ustawianie, ze statek zostal polozony
            var ship = Game.Player.Ships.First(s => s.Length == LastGrap.Length && !s.IsSet);
            ship.X = LastGrap.X;
            ship.Y = LastGrap.Y;
            ship.Direction = LastGrap.Direction;
            ship.IsSet = true;
            LastGrap.AvailableFields.ForEach(p => p.TypeOfField = KindOfField.Ship);
            LastGrap.QuantityOfUnsituated--;

            //Wypisanie powyzszego
            int row = 5 - LastGrap.Length;
            var label = InformationGrid.Children.OfType<Label>().First(l => Grid.GetRow(l) == row);
            label.Content = "x " + LastGrap.QuantityOfUnsituated;

            //Resetowanie ostatniego chwytania(?) 
            LastGrap.IsGrabbed = false;
            LastGrap.Length = 0;
            LastGrap.AvailableFields = null;
            LastGrap.FieldsOfPotentialShip = null;

            //Jesli ustawiono wszystkie statki to mozna kontynuowac
            bool isSet = Game.Player.CheckIfSet();
            ActionButton.IsEnabled = isSet; 

            SetCursorOfBoard(PlayerBoard, Cursors.Arrow);
            SetCursorOfBoard(InformationGrid, isSet ? Cursors.Arrow : Cursors.Hand);
            
        }

        public void Image_EnteringCursorOnField(object sender, RoutedEventArgs e)
        {
            //Czy jest chwycony statek
            if (GameState != State.Preordering || !LastGrap.IsGrabbed)
                return;

            //Znalezienie odpowiedniego pola
            var image = (Image) sender;
            int y = Grid.GetColumn(image);
            int x = Grid.GetRow(image);

            //Zapamietanie ostatniego miejsca kursora
            LastGrap.X = x;
            LastGrap.Y = y;

            //Zbiera pola na ktorych bedzie zmieniany kolor
            switch (LastGrap.Direction)
            {
                case Direction.Down:
                    LastGrap.FieldsOfPotentialShip =
                        Game.Player.UsersBoard.ListOfFields.Where(
                            p => p.X == x && p.Y >= y && p.Y <= y + LastGrap.Length - 1).ToList();
                    break;
                case Direction.Right:
                    LastGrap.FieldsOfPotentialShip =
                        Game.Player.UsersBoard.ListOfFields.Where(
                            p => p.Y == y && p.X >= x && p.X <= x + LastGrap.Length - 1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //Znajduje okolice potencjalnego statku
            var surround = Game.Player.UsersBoard.FindSurround(LastGrap.FieldsOfPotentialShip);

            //Ze wszystkich potencjalnych pol wybieramy tylko te puste (tam gdzie nie ma statku)
            LastGrap.AvailableFields =
                LastGrap.FieldsOfPotentialShip.Where(p => p.TypeOfField == KindOfField.Empty).ToList();

            //Sprawdza czy mozna postawic statek
            bool possibleToPlace = LastGrap.AvailableFields.Count == LastGrap.Length && !surround.Exists(p => p.TypeOfField == KindOfField.Ship);
            LastGrap.IsPossibleToPlace = possibleToPlace;

            //zmienia kursor i kolor pol w zaleznosci czy mozna postawic statek
            image.Cursor = possibleToPlace ? Cursors.Hand : Cursors.Arrow;
            foreach (var field in LastGrap.FieldsOfPotentialShip)
            {
                var element = PlayerBoard.Children.OfType<Image>().First(i => Grid.GetRow(i) == field.X && Grid.GetColumn(i) == field.Y);
                element.Source = new BitmapImage(UriPossibilitiesToPlaceShip[possibleToPlace]);
            }

        }

        public void Image_LeavingCursorFromField(object sender, RoutedEventArgs e)
        {
            //Czy statek jest chwycony
            if (GameState != State.Preordering || !LastGrap.IsGrabbed)
                return;
            
            //Cofamy zmiany, tak ze opuszczane pola zostana takie jak przed wjechaniem myszy
            foreach (var field in LastGrap.FieldsOfPotentialShip)
            {
                var element = PlayerBoard.Children.OfType<Image>().First(i => Grid.GetRow(i) == field.X && Grid.GetColumn(i) == field.Y);
                element.Source = new BitmapImage(UrisImagesToGame[field.TypeOfField]);
            }
            var image = (Image) sender;
            image.Cursor = Cursors.Arrow;
            //Ustawic pola na neutralne
        }

        private void EndGame(User user)
        {
            GameState = State.End;
            Show();
            ActionButton.Content = "KONIEC";
            ActionButton.IsEnabled = true;
            SetCursorOfBoard(ComputerBoard, Cursors.Arrow);
            SetCursorOfBoard(PlayerBoard, Cursors.Arrow);
            
            //Sprawdzanie ktory gracz wygral
            if (user is Player)
            {
                Instructions.Text = "Brawo!\nRozwliłeś komputera.";
            }
            else if(user is Computer)
            {
                Instructions.Text = "Beznadziejnie!\nKomputer z Tobą wygrał.";

            }
            else
            {
                throw new Exception("Niemozliwy typ uzytkownika");
            }
        }

        public void UpdateComputerStatistics()
        {
            var ships = Game.Player.Ships;
            PlayerQuantityOfOneDecker.Content = "x " + ships.Count(s => s.Length == 1 && s.IsSinked == false);
            PlayerQuantityOfTwoDecker.Content = "x " + ships.Count(s => s.Length == 2 && s.IsSinked == false);
            PlayerQuantityOfThreeDecker.Content = "x " + ships.Count(s => s.Length == 3 && s.IsSinked == false);
            PlayerQuantityOfFourDecker.Content = "x " + ships.Count(s => s.Length == 4 && s.IsSinked == false);
        }

        public void UpdatePlayerStatistics()
        {
            var ships = Game.Pc.Ships;
            ComputerQuantityOfOneDecker.Content = "x " + ships.Count(s => s.Length == 1 && s.IsSinked == false);
            ComputerQuantityOfTwoDecker.Content = "x " + ships.Count(s => s.Length == 2 && s.IsSinked == false);
            ComputerQuantityOfThreeDecker.Content = "x " + ships.Count(s => s.Length == 3 && s.IsSinked == false);
            ComputerQuantityOfFourDecker.Content = "x " + ships.Count(s => s.Length == 4 && s.IsSinked == false);
        }

        private async void ComputerMove()
        {

            while (true)
            {
                if (GameState != State.ComputerMove)
                    return;

                //Opoznienie ruchu komputera
                await Task.Delay(800);

                //Jesli komputer zatopil ostatni statek
                if (Game.ComputerMove())
                {
                    Informations.Text = "Trafiony Zatopiony!";
                    UpdateComputerStatistics();
                    EndGame(Game.Pc);
                    return;
                }

                //Trafil to kontynuuje gre
                if (Game.Pc.LastAction == ActionAfterShot.Hit ||
                         Game.Pc.LastAction == ActionAfterShot.Sinked)
                {
                    UpdateComputerStatistics();
                    Informations.Text = Game.Pc.LastAction == ActionAfterShot.Hit ?
                                        "Trafiony!" : "Trafiony Zatopiony!";
                    GameState = State.ComputerMove;
                    ActionButton.Content = "Ruch Komputera";
                    Show(false);
                }
                else
                {
                    GameState = State.PlayerMove;
                    Informations.Text = "Pudło!";
                    ActionButton.Content = "Twój Ruch";
                    Show();
                    break;
                }
            }
        }

        //Event po klikniecu Wielkiego Zielonego klawisza
        private void Przebieg_Click(object sender, RoutedEventArgs e)
        {
            switch (GameState)
            {
                case State.Start:
                    GameState = State.Preordering;
                    ShowInfromationToSettingShips();
                    break;

                case State.Preordering:
                    HideInformationToSettingsShips();
                    GameState = State.PlayerMove;
                    Game.Pc.PickShips();
                    Show();
                    ActionButton.IsEnabled = false;
                    ActionButton.Content = "Twój Ruch";
                    Instructions.Text = "Intrukcje:\nZaznacz pole na planszy komputera gdzie chcesz strzelić";
                    AssignMouseButtonEventToBoard(ComputerBoard, Image_ShootingInComputerFields, MouseLeftButtonDownEvent);
                    break;

                case State.End:
                    Application.Current.Shutdown();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (GameState != State.Preordering)
                return;

            Game.Player.Reset();
            SetCursorOfBoard(InformationGrid, Cursors.Hand);

            var listOfLabelsWithQuantityOfShips = InformationGrid.Children.OfType<Label>().ToList();
            foreach(var label in listOfLabelsWithQuantityOfShips)
            {
                label.Content = "x " + Grid.GetRow(label);
            }

            ActionButton.IsEnabled = false;
            Show(false);
        }

        private void Losuj_Click(object sender, RoutedEventArgs e)
        {
            if (GameState != State.Preordering)
                return;

            Game.Player.PickShips();

            SetCursorOfBoard(PlayerBoard, Cursors.Arrow);
            SetCursorOfBoard(InformationGrid, Cursors.Arrow);
            InformationGrid.Children.OfType<Label>().ToList().ForEach(l => l.Content = "x 0");
            ActionButton.IsEnabled = true;
            Show(false);
        }

        private void Obroc_Click(object sender, RoutedEventArgs e)
        {
            if (GameState != State.Preordering || !LastGrap.IsGrabbed)
                return;

            LastGrap.Direction = LastGrap.Direction == Direction.Down ? Direction.Right : Direction.Down;
        }
    }
    
}
