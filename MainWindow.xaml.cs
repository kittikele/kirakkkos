using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MERET = 3;
        BitmapImage eredetiKep;

        UIElement huzottElem;
        Point egérEltolás;

        TextBox[,] sudokuMezok = new TextBox[9, 9];
        public MainWindow()
        {

            InitializeComponent();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            if (menu == null) return;

            switch (menu.Header.ToString())
            {
                case "Helyszín":
                    MessageBox.Show("Szegedi SzC Vasvári Pál Technikum és otthon.");
                    break;
                case "Mikor":
                    MessageBox.Show("A 2025-26-os tanévben készült.");
                    break;
                case "Kik":
                    MessageBox.Show("Kele Kitti (12.c), Fleisz Lajos Orbán (12.c), Nagy Marcell (12.c) készítette.");
                    break;
            }
        }

        private void KepValaszto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Képfájlok|*.jpg;*.png;*.bmp"
            };

            if (ofd.ShowDialog() == true)
            {
                eredetiKep = new BitmapImage(new Uri(ofd.FileName));
                kepHelye.Source = eredetiKep;   // halvány jó kép
                KirakoLetrehoz();
            }
        }
        private void KirakoLetrehoz()
        {
            PuzzleCanvas.Children.Clear();

            int canvasW = (int)PuzzleCanvas.ActualWidth;
            int canvasH = (int)PuzzleCanvas.ActualHeight;

            BitmapSource fillKep = KepFillMeretre(eredetiKep, canvasW, canvasH);

            int darabW = canvasW / MERET;
            int darabH = canvasH / MERET;

            Random rnd = new Random();

            for (int sor = 0; sor < MERET; sor++)
            {
                for (int oszlop = 0; oszlop < MERET; oszlop++)
                {
                    CroppedBitmap darab = new CroppedBitmap(
                        fillKep,
                        new Int32Rect(oszlop * darabW, sor * darabH, darabW, darabH));

                    Image img = new Image
                    {
                        Source = darab,
                        Width = darabW,
                        Height = darabH,
                        Stretch = Stretch.Fill,

                        // HELYES POZÍCIÓ
                        Tag = new Point(oszlop * darabW, sor * darabH)
                    };

                    Canvas.SetLeft(img, rnd.Next(0, canvasW - darabW));
                    Canvas.SetTop(img, rnd.Next(0, canvasH - darabH));

                    img.MouseLeftButtonDown += Darab_MouseDown;
                    img.MouseMove += Darab_MouseMove;
                    img.MouseLeftButtonUp += Darab_MouseUp;

                    PuzzleCanvas.Children.Add(img);
                    Panel.SetZIndex(img, 1); // alapból a canvas tetején 
                }
            }
        }
        private void Darab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            huzottElem = sender as UIElement;

            // FONTOS: az egér pozíciója A DARABON BELÜL
            egérEltolás = e.GetPosition(huzottElem);

            Panel.SetZIndex(huzottElem, 10);
            huzottElem.CaptureMouse();
        }
        private void Darab_MouseMove(object sender, MouseEventArgs e)
        {
            if (huzottElem == null) return;

            Point p = e.GetPosition(PuzzleCanvas);

            Canvas.SetLeft(huzottElem, p.X - egérEltolás.X);
            Canvas.SetTop(huzottElem, p.Y - egérEltolás.Y);
        }
        private void Darab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (huzottElem == null) return;

            huzottElem.ReleaseMouseCapture();

            // vissza alap szintre
            Panel.SetZIndex(huzottElem, 1);

            Image img = huzottElem as Image;
            Point helyes = (Point)img.Tag;

            double x = Canvas.GetLeft(img);
            double y = Canvas.GetTop(img);

            if (Math.Abs(x - helyes.X) < 20 && Math.Abs(y - helyes.Y) < 20)
            {
                Canvas.SetLeft(img, helyes.X);
                Canvas.SetTop(img, helyes.Y);

                Panel.SetZIndex(img, -1);        // CANVAS ALJÁRA
                img.IsHitTestVisible = false;    // ne legyen mozgatható
            }
            else
            {
                Panel.SetZIndex(img, 1); // marad a többi felett
            }

            huzottElem = null;
            Ellenorzes();
        }
        private void Ellenorzes()
        {
            foreach (UIElement elem in PuzzleCanvas.Children)
            {
                Image img = elem as Image;
                Point helyes = (Point)img.Tag;

                if (Math.Abs(Canvas.GetLeft(img) - helyes.X) > 1 ||
                    Math.Abs(Canvas.GetTop(img) - helyes.Y) > 1)
                    return;
            }

            MessageBox.Show("Ügyes volt! 🎉");
        }
        private void Tema_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            if (menu == null) return;

            switch (menu.Header.ToString())
            {
                case "Gyümölcs":
                    valasztott.Header = "Gyümölcs";
                    break;
                case "Zöldség":
                    valasztott.Header = "Zöldség";
                    break;
                case "Ország":
                    valasztott.Header = "Ország";
                    break;
            }
        }
        private RenderTargetBitmap KepFillMeretre(BitmapSource forras, int w, int h)
        {
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(forras, new Rect(0, 0, w, h));
            }

            RenderTargetBitmap rtb =
                new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);

            rtb.Render(dv);
            return rtb;
        }
        private void KirakoMenu_Click(object sender, RoutedEventArgs e)
        {
            SudokuGrid.Visibility = Visibility.Collapsed;
            PuzzleCanvas.Visibility = Visibility.Visible;
            kepHelye.Visibility = Visibility.Visible;
        }

        private void SudokuMenu_Click(object sender, RoutedEventArgs e)
        {
            PuzzleCanvas.Visibility = Visibility.Collapsed;
            kepHelye.Visibility = Visibility.Collapsed;
            SudokuGrid.Visibility = Visibility.Visible;

            SudokuLetrehoz();
        }
        int[,] teljesSudoku =
{
    {5,3,4,6,7,8,9,1,2},
    {6,7,2,1,9,5,3,4,8},
    {1,9,8,3,4,2,5,6,7},
    {8,5,9,7,6,1,4,2,3},
    {4,2,6,8,5,3,7,9,1},
    {7,1,3,9,2,4,8,5,6},
    {9,6,1,5,3,7,2,8,4},
    {2,8,7,4,1,9,6,3,5},
    {3,4,5,2,8,6,1,7,9}
};
        private void SudokuLetrehoz()
        {
            SudokuGrid.Children.Clear();
            SudokuGrid.RowDefinitions.Clear();
            SudokuGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 9; i++)
            {
                SudokuGrid.RowDefinitions.Add(new RowDefinition());
                SudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int[,] kevert = SudokuKever(teljesSudoku);
            int[,] feladvany = FeladvanyKeszit(kevert, 45); // közepes

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = new TextBox
                    {
                        FontSize = 20,
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        BorderThickness = new Thickness(
                            c % 3 == 0 ? 2 : 1,
                            r % 3 == 0 ? 2 : 1,
                            1, 1)
                    };

                    if (feladvany[r, c] != 0)
                    {
                        tb.Text = feladvany[r, c].ToString();
                        tb.IsReadOnly = true;
                        tb.Background = Brushes.LightGray;
                    }

                    Grid.SetRow(tb, r);
                    Grid.SetColumn(tb, c);
                    SudokuGrid.Children.Add(tb);
                }
            }
        }
        private int[,] FeladvanyKeszit(int[,] megoldas, int uresMezok)
        {
            int[,] tabla = (int[,])megoldas.Clone();

            int torolt = 0;
            while (torolt < uresMezok)
            {
                int r = rnd.Next(9);
                int c = rnd.Next(9);

                if (tabla[r, c] != 0)
                {
                    tabla[r, c] = 0;
                    torolt++;
                }
            }
            return tabla;
        }
        Random rnd = new Random();

        private int[,] SudokuKever(int[,] tabla)
        {
            int[,] uj = (int[,])tabla.Clone();

            // sorcsoportok keverése
            for (int i = 0; i < 9; i += 3)
                KeverSorok(uj, i);

            // oszlopcsoportok keverése
            for (int i = 0; i < 9; i += 3)
                KeverOszlopok(uj, i);

            return uj;
        }

        private void KeverSorok(int[,] t, int start)
        {
            for (int i = 0; i < 3; i++)
            {
                int r1 = start + i;
                int r2 = start + rnd.Next(3);

                for (int c = 0; c < 9; c++)
                    (t[r1, c], t[r2, c]) = (t[r2, c], t[r1, c]);
            }
        }

        private void KeverOszlopok(int[,] t, int start)
        {
            for (int i = 0; i < 3; i++)
            {
                int c1 = start + i;
                int c2 = start + rnd.Next(3);

                for (int r = 0; r < 9; r++)
                    (t[r, c1], t[r, c2]) = (t[r, c2], t[r, c1]);
            }
        }
    }
}