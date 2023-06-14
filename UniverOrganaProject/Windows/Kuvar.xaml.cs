using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;
using UniverOrganaProject.Modeli;

namespace UniverOrganaProject.Windows
{
    /// <summary>
    /// Interaction logic for Kuvar.xaml
    /// </summary>
    public partial class Kuvar : Window
    {
        private string korisnickoIme;
        public Kuvar(string korisnickoIme)
        {
            this.korisnickoIme = korisnickoIme;
            DataContext = this;
            InitializeComponent();
            btnMeni(null, null);
        }

        private void btnKuvarMagacin(object sender, RoutedEventArgs e)
        {
            KuvarMagacin userControlKuvarMagacin = new KuvarMagacin();


            kuvarGrid.Children.Clear();
            kuvarGrid.Children.Add(userControlKuvarMagacin);
        }

        private void btnMeni(object sender, RoutedEventArgs e)
        {
            KuvarMeni userControlKuvarMeni = new KuvarMeni();

            kuvarGrid.Children.Clear();
            kuvarGrid.Children.Add(userControlKuvarMeni);
        }

        private void btnRecepti(object sender, RoutedEventArgs e)
        {
            KuvarRecepti userControlKuvarRecepti = new KuvarRecepti();

            kuvarGrid.Children.Clear();
            kuvarGrid.Children.Add(userControlKuvarRecepti);
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUser(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = ((Button)sender).ContextMenu;
            Zaposleni korisnik = DohvatiInformacijeOKorisniku(korisnickoIme);
            contextMenu.DataContext = korisnik;
            contextMenu.IsOpen = true;
        }
        private Zaposleni DohvatiInformacijeOKorisniku(string korisnickoIme)
        {
            Zaposleni korisnik = new Zaposleni();

            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT Name, Surename FROM tblUser WHERE Username = @korisnickoIme", connection);
                command.Parameters.AddWithValue("@korisnickoIme", korisnickoIme);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    korisnik.Name = reader["Name"].ToString();
                    korisnik.Surename = reader["Surename"].ToString();
                }
            }

            return korisnik;
        }

        private void PromeniSifru_Click(object sender, RoutedEventArgs e)
        {
            PromeniSifru.IsOpen = true;
        }

        private void BtnPotvrdi_Click(object sender, RoutedEventArgs e)
        {
            string novaSifra = new System.Net.NetworkCredential(string.Empty, txtNovaŠifraPopup.SecurePassword).Password;
            string potvrdaSifre = new System.Net.NetworkCredential(string.Empty, txtPotvrdaNovaŠifraPopup.SecurePassword).Password;

            if (novaSifra != potvrdaSifre)
            {
                MessageBox.Show("Nova šifra i potvrda šifre se ne podudaraju.");
                return;
            }

            if (PromeniSifruUBazi(novaSifra))
            {
                MessageBox.Show("Šifra uspešno promenjena.");
            }
            else
            {
                MessageBox.Show("Došlo je do greške prilikom promene šifre.");
            }

            PromeniSifru.IsOpen = false;
            Close();
        }

        private void BtnOtkazi_Click(object sender, RoutedEventArgs e)
        {
            PromeniSifru.IsOpen = false;
        }

        private bool PromeniSifruUBazi(string novaSifra)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("UPDATE tblUser SET Password = @novaSifra WHERE Username = @korisnickoIme", connection);
                command.Parameters.AddWithValue("@novaSifra", novaSifra);
                command.Parameters.AddWithValue("@korisnickoIme", korisnickoIme);

                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }
    }
}
