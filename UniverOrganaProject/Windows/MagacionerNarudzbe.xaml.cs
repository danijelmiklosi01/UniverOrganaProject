using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UniverOrganaProject.Windows
{
    /// <summary>
    /// Interaction logic for MagacionerNarudzbe.xaml
    /// </summary>
    public partial class MagacionerNarudzbe : Window
    {

        private string korisnickoIme;
        public MagacionerNarudzbe(string korisnickoIme)
        {
            InitializeComponent();
            DataContext = this;
            this.korisnickoIme = korisnickoIme;
        }
        private void btnHelp(object sender, RoutedEventArgs e)
        {
            string helpFilePath = "Res/magacionerHelp/magacionerNarudzbeHelp.html";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(helpFilePath.Replace('/', '.')));

            if (resourceName != null)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string htmlContent = reader.ReadToEnd();
                        try
                        {
                            // Kreiranje privremene HTML datoteke
                            string tempHtmlFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "loginHelp.html");
                            File.WriteAllText(tempHtmlFilePath, htmlContent);


                            // Otvaranje privremene HTML datoteke u podrazumevanom web pregledaču
                            Process.Start(tempHtmlFilePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Došlo je do greške prilikom otvaranja HTML sadržaja: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Nije moguće pronaći resurs '" + helpFilePath + "'.");
            }
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnStampaj(object sender, RoutedEventArgs e)
        {

        }

        private void btnBack(object sender, RoutedEventArgs e)
        {
            Magacioner magacioner = new Magacioner(korisnickoIme);
            magacioner.Show();
            Close();
        }


    }
}
