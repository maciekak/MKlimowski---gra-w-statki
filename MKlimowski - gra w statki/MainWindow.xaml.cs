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
        private Stan StanGry { get; }
        private Dictionary<RodzajPola, Uri> UriObrazkow { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Instukcje.Text = "Instrukcje:\nRozmiesc swoje statki.";

            MainGrid.ShowGridLines = false; // TODO: temporary

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

            //TODO: temporary
            //plansza.ShowGridLines = true;
        }

        private void Rysuj()
        {
            Action EmptyDelegate = delegate() { };
            //Rysuj plansze Gracza
            foreach (var pole in Gra.Gracza.ListaPol)
            {
                var element = PlanszaGracza.Children.Cast<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriObrazkow[pole.TypPola]);
                //element.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            }

            //Rysuj plansze komputera
            foreach (var pole in Gra.Komputera.ListaPol)
            {
                var element = PlanszaGracza.Children.Cast<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                //Jesli polem jest statek, to ustaw jako puste, zeby gracz nie widzial gdzie statki ma komputer
                element.Source = pole.TypPola == RodzajPola.Statek ? new BitmapImage(UriObrazkow[RodzajPola.Puste]) : new BitmapImage(UriObrazkow[pole.TypPola]);
                //element.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            }
        }

        private void Przebieg_Click(object sender, RoutedEventArgs e)
        {
            switch (StanGry)
            {
                case Stan.Start:
                    Gra.InicjalizujFakowymiStatkami();
                    Rysuj();
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

    //TODO: nie wiem czy ten plik
    public enum Stan
    {
        Start, Rozstawianie, KoniecRozstawiania, RuchGracza, RuchKomputera, Koniec
    }
}
