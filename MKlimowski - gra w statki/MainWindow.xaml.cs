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
        private Stan _stanGry;

        private Stan StanGry
        {
            get
            {
                return _stanGry;
            }
            set
            {
                _stanGry = value;
                switch (_stanGry)
                {
                    case Stan.Start:
                        Przebieg.Content = "START";
                        break;
                    case Stan.Rozstawianie:
                        break;
                    case Stan.KoniecRozstawiania:
                        break;
                    case Stan.RuchGracza:
                        break;
                    case Stan.RuchKomputera:
                        break;
                    case Stan.Koniec:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            Instukcje.Text = "Instrukcje:\nRozmiesc swoje statki.";

            double szerokoscKolumny = MainGrid.ColumnDefinitions[0].Width.Value / Wymiar;
            double wysokoscWiersza = MainGrid.RowDefinitions[1].Height.Value / Wymiar;

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
            }

            //Rysuj plansze komputera
            foreach (var pole in Gra.PrzeciwnikKomputerowy.PlanszaUzytkownika.ListaPol)
            {
                var element = PlanszaKomputera.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                //Jesli polem jest statek, to ustaw jako puste, zeby gracz nie widzial gdzie statki ma komputer
                element.Source = /*pole.TypPola == RodzajPola.Statek ? new BitmapImage(UriObrazkow[RodzajPola.Puste]) :*/ new BitmapImage(UriObrazkow[pole.TypPola]);
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
                    Przebieg.Content = "Rozstawione";
                    break;
                case Stan.Rozstawianie:
                    Gra.InicjalizujFakowymiStatkami();
                    Rysuj();
                    StanGry = Stan.KoniecRozstawiania;
                    break;
                case Stan.KoniecRozstawiania:
                    StanGry = Stan.RuchGracza;
                    Gra.PrzeciwnikKomputerowy.LosujStatki();
                    Rysuj();
                    Przebieg.IsEnabled = false;
                    Przebieg.Content = "Zakończ swój ruch";
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
    }

    //TODO: nie wiem czy ten plik
    public enum Stan
    {
        Start, Rozstawianie, KoniecRozstawiania, RuchGracza, RuchKomputera, Koniec
    }
}
