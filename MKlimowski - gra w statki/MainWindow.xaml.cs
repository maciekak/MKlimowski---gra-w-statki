using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    //TODO: zmienic nazwy na angielskie
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int Wymiar = 10; //TODO: nie ten plik
        private Rozgrywka Gra { get; }
        private Dictionary<RodzajPola, Uri> UriObrazkowDoGry { get; set; }
        private Dictionary<bool, Uri> UriCzyMoznaPolozycStatek { get; set; }
        private Stan StanGry { get; set; }
        private DaneChwytaniaStatkow OstatniChwytany { get; }

        public MainWindow()
        {
            InitializeComponent();
            Instukcje.Text = "Instrukcje:\nRozmiesc swoje statki.";
            
            double szerokoscKolumny = MainGrid.ColumnDefinitions[0].Width.Value / Wymiar;
            double wysokoscWiersza = (MainGrid.RowDefinitions[1].Height.Value) / Wymiar;

            UstawObrazkiPol();

            this.StworzPlansze(PlanszaKomputera, szerokoscKolumny, wysokoscWiersza);
            this.StworzPlansze(PlanszaGracza, szerokoscKolumny, wysokoscWiersza);
            
            JednomasztG.Source = Jednomaszt.Source = Jednomasztowiec.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Statek.png"));
            DwumasztG.Source = Dwumaszt.Source = Dwumasztowiec.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\2xStatek.png"));
            TrojmasztG.Source = Trojmaszt.Source = Trojmasztowiec.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\3xStatek.png"));
            CzteromasztG.Source = Czteromaszt.Source = Czteromasztowiec.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\4xStatek.png"));

            OstatniChwytany = new DaneChwytaniaStatkow();
            Gra = new Rozgrywka();
            StanGry = Stan.Start;
        }

        private void UstawObrazkiPol()
        {
            UriObrazkowDoGry = new Dictionary<RodzajPola, Uri>()
            {
                {RodzajPola.Puste, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png")},
                {RodzajPola.Pudlo, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Pudlo.png")},
                {RodzajPola.Trafiony, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Trafiony.png")},
                {RodzajPola.Statek, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Statek.png")}
            };

            UriCzyMoznaPolozycStatek = new Dictionary<bool, Uri>()
            {
                {true, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\PropozycjaStatek.png")},
                {false, new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\NiedozwolonyStatek.png")}
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
                    pole.SetValue(Grid.RowProperty, x);
                    pole.SetValue(Grid.ColumnProperty, y);
                    plansza.Children.Add(pole);
                }
            }
            
        }

        public void Rysuj(bool czyUstawiacKursorLapki = true)
        {
            var kursor = czyUstawiacKursorLapki ? Cursors.Hand : Cursors.Arrow;

            //Rysuj plansze Gracza
            foreach (var pole in Gra.Player.PlanszaUzytkownika.ListaPol)
            {
                var element = PlanszaGracza.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriObrazkowDoGry[pole.TypPola]);
            }

            //Rysuj plansze komputera
            foreach (var pole in Gra.PrzeciwnikKomputerowy.PlanszaUzytkownika.ListaPol)
            {
                var element = PlanszaKomputera.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                //Jesli polem jest statek, to ustaw jako puste, zeby gracz nie widzial gdzie statki ma komputer
                element.Source = pole.TypPola == RodzajPola.Statek ? new BitmapImage(UriObrazkowDoGry[RodzajPola.Puste]) : new BitmapImage(UriObrazkowDoGry[pole.TypPola]);
                element.Cursor = pole.TypPola == RodzajPola.Pudlo || pole.TypPola == RodzajPola.Trafiony
                    ? Cursors.Arrow
                    : kursor;
            }
        }

        public static void PrzypiszEventPrzyciskuPlanszy(Grid grid, MouseButtonEventHandler mouseButtonEventHandler, RoutedEvent routedEvent)
        {
            var listaObrazkow = grid.Children.OfType<Image>().ToList();
            listaObrazkow.ForEach(i => i.AddHandler(routedEvent, mouseButtonEventHandler)); //TODO: Mouse down temporary
        }

        public static void PrzypiszEventMyszyPlanszy(Grid grid, MouseEventHandler mouseEventHandler, RoutedEvent routedEvent)
        {
            var listaObrazkow = grid.Children.OfType<Image>().ToList();
            listaObrazkow.ForEach(i => i.AddHandler(routedEvent, mouseEventHandler)); //TODO: Mouse down temporary
        }

        public void PokazInformacjeDoUstawianiaStatkow()
        {
            Jednomasztowiec.IsEnabled = true;
            Jednomasztowiec.Visibility = Visibility.Visible;
            OpisJednomasztowca.Visibility = Visibility.Visible;
            RamkaJednomasztowca.Visibility = Visibility.Visible;

            Dwumasztowiec.IsEnabled = true;
            Dwumasztowiec.Visibility = Visibility.Visible;
            OpisDwumasztowca.Visibility = Visibility.Visible;
            RamkaDwumasztowca.Visibility = Visibility.Visible;

            Trojmasztowiec.IsEnabled = true;
            Trojmasztowiec.Visibility = Visibility.Visible;
            OpisTrojmasztowca.Visibility = Visibility.Visible;
            RamkaTrojmasztowca.Visibility = Visibility.Visible;

            Czteromasztowiec.IsEnabled = true;
            Czteromasztowiec.Visibility = Visibility.Visible;
            OpisCzteromasztowca.Visibility = Visibility.Visible;
            RamkaCzteromasztowca.Visibility = Visibility.Visible;

            Losuj.Visibility = Visibility.Visible;
            Reset.Visibility = Visibility.Visible;
            Obroc.Visibility = Visibility.Visible;

            Przebieg.Content = "Rozstawione";
            Przebieg.IsEnabled = false;
            Informacje.Text = "Statki:";

            UstawKursorPlanszy(InformacyjnyGrid, Cursors.Hand);

            PrzypiszEventPrzyciskuPlanszy(InformacyjnyGrid, Image_Chwytanie, MouseDownEvent);
            PrzypiszEventPrzyciskuPlanszy(PlanszaGracza, Image_UpuszczanieStatku, MouseDownEvent);

            PrzypiszEventMyszyPlanszy(PlanszaGracza, Image_WjezdzanieKursoraNaPole, MouseEnterEvent);
            PrzypiszEventMyszyPlanszy(PlanszaGracza, Image_WyjezdzanieKursoraZPola, MouseLeaveEvent);
        }

        public void SchowajInformacjeDoUstawiania()
        {
            Jednomasztowiec.IsEnabled = false;
            Jednomasztowiec.Visibility = Visibility.Hidden;
            OpisJednomasztowca.Visibility = Visibility.Hidden;
            RamkaJednomasztowca.Visibility = Visibility.Hidden;

            Dwumasztowiec.IsEnabled = false;
            Dwumasztowiec.Visibility = Visibility.Hidden;
            OpisDwumasztowca.Visibility = Visibility.Hidden;
            RamkaDwumasztowca.Visibility = Visibility.Hidden;

            Trojmasztowiec.IsEnabled = false;
            Trojmasztowiec.Visibility = Visibility.Hidden;
            OpisTrojmasztowca.Visibility = Visibility.Hidden;
            RamkaTrojmasztowca.Visibility = Visibility.Hidden;

            Czteromasztowiec.IsEnabled = false;
            Czteromasztowiec.Visibility = Visibility.Hidden;
            OpisCzteromasztowca.Visibility = Visibility.Hidden;
            RamkaCzteromasztowca.Visibility = Visibility.Hidden;

            Losuj.Visibility = Visibility.Hidden;
            Reset.Visibility = Visibility.Hidden;
            Obroc.Visibility = Visibility.Hidden;

            UstawKursorPlanszy(InformacyjnyGrid, Cursors.Arrow);
            UstawKursorPlanszy(PlanszaGracza, Cursors.Arrow);
            Informacje.Text = "";
            
            PokazStatystyki();
        }

        public void PokazStatystyki()
        {
            JednomasztG.Visibility = Visibility.Visible;
            DwumasztG.Visibility = Visibility.Visible;
            TrojmasztG.Visibility = Visibility.Visible;
            CzteromasztG.Visibility = Visibility.Visible;

            Jednomaszt.Visibility = Visibility.Visible;
            Dwumaszt.Visibility = Visibility.Visible;
            Trojmaszt.Visibility = Visibility.Visible;
            Czteromaszt.Visibility = Visibility.Visible;

            IleJednomasztG.Visibility = Visibility.Visible;
            IleDwumasztG.Visibility = Visibility.Visible;
            IleTrojmasztG.Visibility = Visibility.Visible;
            IleCzteromasztG.Visibility = Visibility.Visible;

            IleJednomaszt.Visibility = Visibility.Visible;
            IleDwumaszt.Visibility = Visibility.Visible;
            IleTrojmaszt.Visibility = Visibility.Visible;
            IleCzteromaszt.Visibility = Visibility.Visible;

            StatystkiGracz.Visibility = Visibility.Visible;
            StatystkiKomputer.Visibility = Visibility.Visible;
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
                    StanGry = Stan.RuchGracza;
                    break;
                case AkcjaPoStrzale.Pudlo:
                    Przebieg.Content = "Ruch Komputera";
                    Informacje.Text = "Pudło!";
                    Rysuj(false);
                    StanGry = Stan.RuchKomputera;
                    RuchKomputera();
                    break;
                case AkcjaPoStrzale.Zatopiony:
                    Informacje.Text = "Trafiony Zatopiony!";
                    AkutalizujStatystykiGracza();
                    StanGry = Stan.RuchGracza;
                    //Jesli Gracz wygral
                    if (Gra.PrzeciwnikKomputerowy.CzyKoniec())
                    {
                        ZakonczGre(Gra.Player);
                    }
                    break;
                case AkcjaPoStrzale.Blad:
                    Informacje.Text = "Tu już strzelałeś";
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Rysuj(StanGry == Stan.RuchGracza);
        }

        public void Image_Chwytanie(object sender, RoutedEventArgs e)
        {
            //Sprawdzic czy mozna chwycic
            if (StanGry != Stan.Rozstawianie || OstatniChwytany.CzyChwycono) return;

            int dlugosc = ZnajdzDlugoscStatku(sender as Image);
            int ilosc = Gra.Player.Statki.Count(s => s.Dlugosc == dlugosc && s.CzyUstawiony == false);
            if (ilosc <= 0) return;

            OstatniChwytany.Dlugosc = dlugosc;
            OstatniChwytany.CzyChwycono = true;
            OstatniChwytany.Kierunek = Kierunek.Dol;
            OstatniChwytany.IloscNieustawionychStatkow = ilosc;
            
            UstawKursorPlanszy(InformacyjnyGrid, Cursors.Arrow);
        }

        private static int ZnajdzDlugoscStatku(Image obrazek)
        {
            int wiersz = Grid.GetRow(obrazek);
            if(wiersz > 4 || wiersz < 1) throw new ArgumentException("Niepoprawny wiersz");

            return 5 - wiersz; //bo w pierwszym wierszu mamy czteromasztowca, w drugim troj itp itd

        }

        public void Image_UpuszczanieStatku(object sender, RoutedEventArgs e)
        {
            //Sprawdzic czy mozna
            if (StanGry != Stan.Rozstawianie || !OstatniChwytany.CzyChwycono || !OstatniChwytany.CzyMoznaPostawic)
                return;

            //Zmienic kolor statku
            foreach (var pole in OstatniChwytany.DozwolonePola)
            {
                var element = PlanszaGracza.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriObrazkowDoGry[RodzajPola.Statek]);
            }

            var statek = Gra.Player.Statki.First(s => s.Dlugosc == OstatniChwytany.Dlugosc && !s.CzyUstawiony);
            statek.X = OstatniChwytany.X;
            statek.Y = OstatniChwytany.Y;
            statek.Kierunek = OstatniChwytany.Kierunek;
            statek.CzyUstawiony = true;
            OstatniChwytany.DozwolonePola.ForEach(p => p.TypPola = RodzajPola.Statek);
            OstatniChwytany.IloscNieustawionychStatkow--;

            int wiersz = 5 - OstatniChwytany.Dlugosc;
            var napis = InformacyjnyGrid.Children.OfType<Label>().First(l => Grid.GetRow(l) == wiersz);
            napis.Content = "x " + OstatniChwytany.IloscNieustawionychStatkow;

            OstatniChwytany.CzyChwycono = false;
            OstatniChwytany.Dlugosc = 0;
            OstatniChwytany.DozwolonePola = null;
            OstatniChwytany.KolorowanePola = null;

            bool czyUstawiono = Gra.Player.CzyUstawiono();
            Przebieg.IsEnabled = czyUstawiono; //Jesli ustawiono wszystkie statki to mozna kontynuowac

            UstawKursorPlanszy(PlanszaGracza, Cursors.Arrow);
            UstawKursorPlanszy(InformacyjnyGrid, czyUstawiono ? Cursors.Arrow : Cursors.Hand);

            //Ustawic flage
        }

        public void Image_WjezdzanieKursoraNaPole(object sender, RoutedEventArgs e)
        {
            //Czy jest chwycony statek
            if (StanGry != Stan.Rozstawianie || !OstatniChwytany.CzyChwycono) return;

            var obrazek = (Image) sender;
            int y = Grid.GetColumn(obrazek);
            int x = Grid.GetRow(obrazek);

            OstatniChwytany.X = x;
            OstatniChwytany.Y = y;
            switch (OstatniChwytany.Kierunek)
            {
                case Kierunek.Dol:
                    OstatniChwytany.KolorowanePola =
                        Gra.Player.PlanszaUzytkownika.ListaPol.Where(
                            p => p.X == x && p.Y >= y && p.Y <= y + OstatniChwytany.Dlugosc - 1).ToList();
                    break;
                case Kierunek.Prawo:
                    OstatniChwytany.KolorowanePola =
                        Gra.Player.PlanszaUzytkownika.ListaPol.Where(
                            p => p.Y == y && p.X >= x && p.X <= x + OstatniChwytany.Dlugosc - 1).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var okolica = Gra.Player.PlanszaUzytkownika.ZnajdzOkolice(OstatniChwytany.KolorowanePola);
            OstatniChwytany.DozwolonePola =
                OstatniChwytany.KolorowanePola.Where(p => p.TypPola == RodzajPola.Puste).ToList();

            //Ustawia odpowiednio kolory w zaleznosci od tego czy mozna postawic statek
            var moznaPostawic = OstatniChwytany.DozwolonePola.Count == OstatniChwytany.Dlugosc && !okolica.Exists(p => p.TypPola == RodzajPola.Statek);
            OstatniChwytany.CzyMoznaPostawic = moznaPostawic;

            obrazek.Cursor = moznaPostawic ? Cursors.Hand : Cursors.Arrow;

            foreach (var pole in OstatniChwytany.KolorowanePola)
            {
                var element = PlanszaGracza.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriCzyMoznaPolozycStatek[moznaPostawic]);
            }

        }

        public void Image_WyjezdzanieKursoraZPola(object sender, RoutedEventArgs e)
        {
            //Czy jest chwycony
            if (StanGry != Stan.Rozstawianie || !OstatniChwytany.CzyChwycono) return;
            
            foreach (var pole in OstatniChwytany.KolorowanePola)
            {
                var element = PlanszaGracza.Children.OfType<Image>().First(i => Grid.GetRow(i) == pole.X && Grid.GetColumn(i) == pole.Y); //TODO: Osobna funkcja?
                element.Source = new BitmapImage(UriObrazkowDoGry[pole.TypPola]);
            }
            var obrazek = (Image) sender;
            obrazek.Cursor = Cursors.Arrow;
            //Ustawic pola na neutralne
        }

        private void ZakonczGre(Uzytkownik uzytkownik)
        {
            StanGry = Stan.Koniec;
            Rysuj();
            Przebieg.Content = "KONIEC";
            Przebieg.IsEnabled = true;
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

        public void AkutalizujStatystykiKomputera()
        {
            var statki = Gra.Player.Statki;
            IleJednomasztG.Content = "x " + statki.Count(s => s.Dlugosc == 1 && s.CzyZatopiony == false);
            IleDwumasztG.Content = "x " + statki.Count(s => s.Dlugosc == 2 && s.CzyZatopiony == false);
            IleTrojmasztG.Content = "x " + statki.Count(s => s.Dlugosc == 3 && s.CzyZatopiony == false);
            IleCzteromasztG.Content = "x " + statki.Count(s => s.Dlugosc == 4 && s.CzyZatopiony == false);
        }

        public void AkutalizujStatystykiGracza()
        {
            var statki = Gra.PrzeciwnikKomputerowy.Statki;
            IleJednomaszt.Content = "x " + statki.Count(s => s.Dlugosc == 1 && s.CzyZatopiony == false);
            IleDwumaszt.Content = "x " + statki.Count(s => s.Dlugosc == 2 && s.CzyZatopiony == false);
            IleTrojmaszt.Content = "x " + statki.Count(s => s.Dlugosc == 3 && s.CzyZatopiony == false);
            IleCzteromaszt.Content = "x " + statki.Count(s => s.Dlugosc == 4 && s.CzyZatopiony == false);
        }

        private async void RuchKomputera()
        {

            while (true)
            {
                if (StanGry != Stan.RuchKomputera) return;
                await Task.Delay(100); //TODO: ustawić na normalna wartosc - 800
                if (Gra.RuchKomputera())
                {
                    Informacje.Text = "Trafiony Zatopiony!";
                    AkutalizujStatystykiKomputera();
                    ZakonczGre(Gra.PrzeciwnikKomputerowy);
                    return;
                }
                if (Gra.PrzeciwnikKomputerowy.OstatniaAkcja == AkcjaPoStrzale.Trafiony ||
                         Gra.PrzeciwnikKomputerowy.OstatniaAkcja == AkcjaPoStrzale.Zatopiony)
                {
                    AkutalizujStatystykiKomputera();
                    Informacje.Text = Gra.PrzeciwnikKomputerowy.OstatniaAkcja == AkcjaPoStrzale.Trafiony ?
                                        "Trafiony!" : "Trafiony Zatopiony!";
                    StanGry = Stan.RuchKomputera;
                    Przebieg.Content = "Ruch Komputera";
                    Rysuj(false);
                }
                else
                {
                    StanGry = Stan.RuchGracza;
                    Informacje.Text = "Pudło!";
                    Przebieg.Content = "Twój Ruch";
                    Rysuj();
                    break;
                }
            }
        }

        private void Przebieg_Click(object sender, RoutedEventArgs e)
        {
            switch (StanGry)
            {
                case Stan.Start:
                    StanGry = Stan.Rozstawianie;
                    PokazInformacjeDoUstawianiaStatkow();
                    break;

                case Stan.Rozstawianie:
                    SchowajInformacjeDoUstawiania();
                    StanGry = Stan.RuchGracza;
                    Gra.PrzeciwnikKomputerowy.LosujStatki();
                    Rysuj();
                    Przebieg.IsEnabled = false;
                    Przebieg.Content = "Twój Ruch";
                    Instukcje.Text = "Intrukcje:\nZaznacz pole na planszy komputera gdzie chcesz strzelić";
                    PrzypiszEventPrzyciskuPlanszy(PlanszaKomputera, Image_StrzelanieWPolaKomputera, MouseLeftButtonDownEvent);
                    break;
                case Stan.Koniec:
                    Application.Current.Shutdown();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.Rozstawianie) return;

            Gra.Player.Resetuj();
            UstawKursorPlanszy(InformacyjnyGrid, Cursors.Hand);
            var listaNapisowIlosciStatkow = InformacyjnyGrid.Children.OfType<Label>().ToList();

            foreach(var napis in listaNapisowIlosciStatkow)
            {
                napis.Content = "x " + Grid.GetRow(napis);
            }

            Przebieg.IsEnabled = false;
            Rysuj(false);
        }

        private void Losuj_Click(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.Rozstawianie) return;

            Gra.Player.LosujStatki();
            UstawKursorPlanszy(PlanszaGracza, Cursors.Arrow);
            UstawKursorPlanszy(InformacyjnyGrid, Cursors.Arrow);
            InformacyjnyGrid.Children.OfType<Label>().ToList().ForEach(l => l.Content = "x 0");
            Przebieg.IsEnabled = true;
            Rysuj(false);
        }

        private void Obroc_Click(object sender, RoutedEventArgs e)
        {
            if (StanGry != Stan.Rozstawianie || !OstatniChwytany.CzyChwycono) return;

            OstatniChwytany.Kierunek = OstatniChwytany.Kierunek == Kierunek.Dol ? Kierunek.Prawo : Kierunek.Dol;
        }
    }

    //TODO: nie wiem czy ten plik
    public enum Stan
    {
        Start, Rozstawianie, RuchGracza, RuchKomputera, Koniec
    }

    public class DaneChwytaniaStatkow : IPolozenie
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool CzyChwycono { get; set; }
        public int Dlugosc { get; set; }
        public Kierunek Kierunek { get; set; }
        public List<Pole> KolorowanePola { get; set; }
        public List<Pole> DozwolonePola { get; set; }
        public int IloscNieustawionychStatkow { get; set; }
        public bool CzyMoznaPostawic { get; set; }

        public DaneChwytaniaStatkow()
        {
            KolorowanePola = null;
            DozwolonePola = null;
            CzyChwycono = false;
            CzyMoznaPostawic = false;
            Dlugosc = 0;
            Kierunek = Kierunek.Dol;
        }

    }
    
}
