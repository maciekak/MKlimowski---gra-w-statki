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

namespace MKlimowski___gra_w_statki
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Wymiar = 10;
        public MainWindow()
        {
            InitializeComponent();

            MainGrid.ShowGridLines = true;

            double szerokoscKolumny = MainGrid.ColumnDefinitions[0].Width.Value / Wymiar;
            double wysokoscWiersza = MainGrid.RowDefinitions[1].Height.Value / Wymiar;

            this.StworzPlansze(PlanszaKomputera, szerokoscKolumny, wysokoscWiersza);
            this.StworzPlansze(PlanszaGracza, szerokoscKolumny, wysokoscWiersza);

        }

        private void StworzPlansze(Grid plansza, double szerokosc, double wysokosc)
        {
            //Tworzenie "tabeli"
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
            for (int j = 0; j < Wymiar; j++)
            {
                for (int i = 0; i < Wymiar; i++)
                {
                    var pole = new Image
                    {
                        Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), @"\Images\Nieodkryte-pole.png"))
                    };

                    pole.SetValue(Grid.RowProperty, j);
                    pole.SetValue(Grid.ColumnProperty, i);
                    plansza.Children.Add(pole);
                }
            }

            //TODO: temporary
            //plansza.ShowGridLines = true;
        }
    }
}
