using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
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
using UniverOrganaProject.Modeli;

namespace UniverOrganaProject.Windows
{
    /// <summary>
    /// Interaction logic for MagacionerNarudzbe.xaml
    /// </summary>
    public partial class MagacionerNarudzbe : Window
    {
        private string korisnickoIme;
        private ObservableCollection<Artikli> listaArtikala;
        public ObservableCollection<Artikli> ListaArtikala
        {
            get { return listaArtikala; }
            set
            {
                if (listaArtikala != value)
                {
                    listaArtikala = value;
                    OnPropertyChanged(nameof(ListaArtikala));
                }
            }
        }

 
        public MagacionerNarudzbe(string korisnickoIme)
        {
            InitializeComponent();
            DataContext = this;
            this.korisnickoIme = korisnickoIme;
            LoadDataFromDatabase();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void LoadDataFromDatabase()
        {

            ListaArtikala = GetArtikliFromDatabase();
        }
        public ObservableCollection<Artikli> GetArtikliFromDatabase()
        {
            ObservableCollection<Artikli> artikli = new ObservableCollection<Artikli>();

            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT * FROM tblArtikliKuvar", connection);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Artikli artikal = new Artikli
                    {
                        NazivArtikla = (string)reader["nazivArtikla"],
                        Kolicina = (int)reader["Kolicina"]
                    };

                    artikli.Add(artikal);
                }
            }

            return artikli;
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
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {

                printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;


                printDialog.PrintTicket.PageScalingFactor = 1;


                printDialog.PrintVisual(lstArtikli, "Lista artikala");
            }
        }

        private void btnBack(object sender, RoutedEventArgs e)
        {
            Magacioner magacioner = new Magacioner(korisnickoIme);
            magacioner.Show();
            Close();
        }

        private void Potvrdi_Click(object sender, RoutedEventArgs e)
        {

            Artikli selectedArtikal = (sender as FrameworkElement).DataContext as Artikli;


            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                try
                {
                    connection.Open();


                    string selectQuery = "SELECT TOP 1 * FROM tblArtikli WHERE NazivArtikla = @nazivArtikla ORDER BY RokTrajanja ASC";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    selectCommand.Parameters.AddWithValue("@nazivArtikla", selectedArtikal.NazivArtikla);

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            
                            string nazivArtikla = (string)reader["NazivArtikla"];
                            int kolicina = (int)reader["Kolicina"];
                            DateTime rokTrajanja = (DateTime)reader["RokTrajanja"];

                            
                            int novaKolicina = kolicina - selectedArtikal.Kolicina;
                            if (novaKolicina >= 0)
                            {
                                reader.Close();

                                string updateQuery = "UPDATE tblArtikli SET Kolicina = @novaKolicina WHERE NazivArtikla = @nazivArtikla";
                                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                                updateCommand.Parameters.AddWithValue("@novaKolicina", novaKolicina);
                                updateCommand.Parameters.AddWithValue("@nazivArtikla", nazivArtikla);
                                updateCommand.ExecuteNonQuery();

                                
                                string deleteQuery = "DELETE FROM tblArtikliKuvar WHERE NazivArtikla = @nazivArtikla AND Kolicina = @kolicina";
                                SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                                deleteCommand.Parameters.AddWithValue("@nazivArtikla", selectedArtikal.NazivArtikla);
                                deleteCommand.Parameters.AddWithValue("@kolicina", selectedArtikal.Kolicina);
                                deleteCommand.ExecuteNonQuery();

                                MessageBox.Show("Artikal je uspešno potvrđen.");
                            }
                            else
                            {
                                MessageBox.Show("Nedovoljna količina artikla za potvrdu.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nema dostupnih artikala sa najstarijim rokom trajanja.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom potvrđivanja artikla: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
            LoadDataFromDatabase();
            lstArtikli.ItemsSource = ListaArtikala;
        }


        private void Otkazi_Click(object sender, RoutedEventArgs e)
        {
            
            Artikli selectedArtikal = (sender as FrameworkElement).DataContext as Artikli;

            
            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                try
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM tblArtikliKuvar WHERE NazivArtikla = @nazivArtikla AND Kolicina = @kolicina";
                    SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                    deleteCommand.Parameters.AddWithValue("@nazivArtikla", selectedArtikal.NazivArtikla);
                    deleteCommand.Parameters.AddWithValue("@kolicina", selectedArtikal.Kolicina);
                    deleteCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom otkazivanja artikla: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                    LoadDataFromDatabase();
                    lstArtikli.ItemsSource = ListaArtikala;

                }
            }
            
        }


    }
}
