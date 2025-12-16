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
                Title = "Kép kiválasztása",
                Filter = "Képfájlok|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (ofd.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ofd.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                kepHelye.Source = bitmap; //ez a kép
            }
        }
    }
}