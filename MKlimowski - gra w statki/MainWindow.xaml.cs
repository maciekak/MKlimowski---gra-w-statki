using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MKlimowski___gra_w_statki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int Wymiar = 10; //TODO: nie ten plik
        private Rozgrywka Gra { get; }
        private Dictionary<RodzajPola, Uri> UriObrazkow { get; set; }
        private Stan StanGry { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Instukcje.Text = "Instrukcje:\nRozmiesc swoje statki.";
            
            double szerokoscKolumny = MainGrid.ColumnDefinitions[0].Width.Value / Wymiar;
            double wysokoscWiersza = (MainGrid.RowDefinitions[1].Height.Value + MainGrid.RowDefinitions[2].Height.Value + MainGrid.RowDefinitions[3].Height.Value) / Wymiar;

            UstawObrazkiPol();

            this.StworzPlansze(PlanszaKomputera, szerokoscKolumny, wysokoscWiersza);
            this.StworzPlansze(PlanszaGracza, szerokoscKolumny, wysokoscWiersza);
            
            Gra = new Rozgrywka();
            StanGry = Stan.Start;
        }

        private void UstawObrazkiPol()
        {
            UriObrazkow = new Dictionary<RodzajPola, Uri>()
            {
                {RodzajPola.Puste, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png")},
                {RodzajPola.Pudlo, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Pudlo.png")},
                {RodzajPola.Trafiony, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Trafiony.png")},
                {RodzajPola.Statek, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Statek.png")}
            };
        }

        public static void UstawKursorPlanszy(Grid grid, Cursor kursor)
        {
            var obrazki = grid.Children.OfType<Image>().ToList();
            obrazki.ForEach(o => o.Cursor = kursor);
        }

        private void StworzPlansze(Grid plansza, double szerokosc, double wysokosc)
        {
            //Tworzenie siatki pol
            for (int i = 0; i < Wymiar; i++)
            {
                plansza.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(szerokosc)
                });
                plansza.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(wysokosc)
                });
            }

            //Wypelnianie nietrafionymi polami
            for (int x = 0; x < Wymiar; x++)
            {
                for (int y = 0; y < Wymiar; y++)
                {
                    var pole = new Image
                    {
                        Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png"))
                    };
                    var n = new Button();
                    n.Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage()
                    };
                    pole.SetValue(Grid.RowProperty, x);
                    pole.SetValue(Grid.ColumnProperty, y);
                    plansza.Children.Add(pole);
                }
            }
            
        }

        public void Rysuj()
        {
            //Rysuj plansze Gracza
            foreach (var pole in Gra.Player.PlanszaUzytkownika.ListaPol)
            {
                var element = PlanszaGracza.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriObrazkow[pole.TypPola]);
//                element.Cursor = pole.TypPola == RodzajPola.Pudlo || pole.TypPola == RodzajPola.Trafiony
//                    ? Cursors.Arrow
//                    : Cursors.Hand;
            }

            //Rysuj plansze komputera
            foreach (var pole in Gra.PrzeciwnikKomputerowy.PlanszaUzytkownika.ListaPol)
            {
                var element = PlanszaKomputera.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                //Jesli polem jest statek, to ustaw jako puste, zeby gracz nie widzial gdzie statki ma komputer
                element.Source = /*pole.TypPola == RodzajPola.Statek ? new BitmapImage(UriObrazkow[RodzajPola.Puste]) :*/ new BitmapImage(UriObrazkow[pole.TypPola]); //TODO: odkomentowac
                element.Cursor = pole.TypPola == RodzajPola.Pudlo || pole.TypPola == RodzajPola.Trafiony
                    ? Cursors.Arrow
                    : Cursors.Hand;
            }
        }

        public static void PrzypiszEventPlanszy(Grid grid, Action<object, RoutedEventArgs> eventAction)
        {
            var listaObrazkow = grid.Children.OfType<Image>().ToList();

            listaObrazkow.ForEach(i => i.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(eventAction))); //TODO: Mouse down temporary
        }

        public void Image_StrzelanieWPolaKomputera(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.RuchGracza) return;
            int y = Grid.GetColumn((Image) sender);
            int x = Grid.GetRow((Image) sender);
            var akcjaPoStrzale = Gra.PrzeciwnikKomputerowy.Strzal(x, y);
            switch(akcjaPoStrzale)
            {
                case AkcjaPoStrzale.Trafiony:
                    Informacje.Text = "Trafiony!";
                    break;
                case AkcjaPoStrzale.Pudlo:
                    Informacje.Text = "Pudło!";
                    break;
                case AkcjaPoStrzale.Zatopiony:
                    Informacje.Text = "Trafiony Zatopiony!";
                    //Jesli Gracz wygral
                    if (Gra.PrzeciwnikKomputerowy.CzyKoniec())
                    {
                        ZakonczGre(Gra.Player);
                    }
                    break;
                case AkcjaPoStrzale.Blad:
                    Informacje.Text = "!!!BŁĄD!!!";
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Przebieg.IsEnabled = true;
            Rysuj();
        }

        private void ZakonczGre(Uzytkownik uzytkownik)
        {
            StanGry = Stan.Koniec;
            UstawKursorPlanszy(PlanszaKomputera, Cursors.Arrow);
            UstawKursorPlanszy(PlanszaGracza, Cursors.Arrow);
            if (uzytkownik is Gracz)
            {
                Instukcje.Text = "Brawo!\nRozwliłeś komputera.";
            }
            else if(uzytkownik is Komputer)
            {
                Instukcje.Text = "Beznadziejnie!\nKomputer z Tobą wygrał.";

            }
            else
            {
                throw new Exception("Niemozliwy typ uzytkownika");
            }
        }
        private void Przebieg_Click(object sender, RoutedEventArgs e)
        {
            switch (StanGry)
            {
                case Stan.Start:
                    StanGry = Stan.Rozstawianie;
                    //TODO: zakomentowane bo fejkowe dane
                    //Przebieg.IsEnabled = false;
                    Losuj.Visibility = Visibility.Visible;
                    Reset.Visibility = Visibility.Visible;
                    Przebieg.Content = "Rozstawione";
                    Przebieg.IsEnabled = false;
                    break;

                case Stan.Rozstawianie:
                    Losuj.Visibility = Visibility.Hidden;
                    Reset.Visibility = Visibility.Hidden;
                    Rysuj();
                    StanGry = Stan.KoniecRozstawiania;
                    break;

                case Stan.KoniecRozstawiania:
                    StanGry = Stan.RuchGracza;
                    Gra.PrzeciwnikKomputerowy.LosujStatki();
                    Rysuj();
                    Przebieg.IsEnabled = false;
                    Przebieg.Content = "Zakończ swój ruch";
                    Instukcje.Text = "Intrukcje:\nZaznacz pole na planszy komputera gdzie chcesz strzelić";
                    PrzypiszEventPlanszy(PlanszaKomputera, Image_StrzelanieWPolaKomputera);
                    break;

                case Stan.RuchGracza:
                    Przebieg.IsEnabled = true;
                    Przebieg.Content = "Zakończ ruch komputera";
                    break;

                case Stan.RuchKomputera:
                    //TODO: ustawic to na koniec rozgrywki
//                    Przebieg.IsEnabled = true;
//                    Przebieg.Content = "KONIEC";
                    break;

                case Stan.Koniec:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.Rozstawianie) return;

            Gra.Player.Resetuj();
            Przebieg.IsEnabled = false;
            Rysuj();
        }

        private void Losuj_Click(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.Rozstawianie) return;

            Gra.Player.LosujStatki();
            Przebieg.IsEnabled = true;
            Rysuj();
        }
    }

    //TODO: nie wiem czy ten plik
    public enum Stan
    {
        Start, Rozstawianie, KoniecRozstawiania, RuchGracza, RuchKomputera, Koniec
    }
}
