using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniverOrganaProject.Modeli;

namespace UniverOrganaProject.Windows
{

    
    /// <summary>
    /// Interaction logic for KuvarMagacin.xaml
    /// </summary>
    /// 
    
    public partial class KuvarMagacin : UserControl
    {
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
        public KuvarMagacin()
        {
            InitializeComponent();
            DataContext = this;
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
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = txtSearch.Text;
            List<Artikli> filteredArtikli = ListaArtikala.Where(a => a.NazivArtikla.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            lstArtikli.ItemsSource = filteredArtikli;
        }
        public ObservableCollection<Artikli> GetArtikliFromDatabase()
        {
            ObservableCollection<Artikli> artikli = new ObservableCollection<Artikli>();

            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT * FROM tblArtikli", connection);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Artikli artikal = new Artikli
                    {
                        NazivArtikla = (string)reader["nazivArtikla"],
                        RokTrajanja = (DateTime)reader["rokTrajanja"],
                        Kolicina = (int)reader["Kolicina"]
                    };

                    artikli.Add(artikal);
                }
            }

            return artikli;
        }
        private void btnNaruci(object sender, RoutedEventArgs e)
        {
            popupNaruciArtikal.IsOpen = true;
        }
        private void BtnNaruciPopup_Click(object sender, RoutedEventArgs e)
        {

            Artikli odabraniArtikal = (Artikli)cmbArtikli.SelectedItem;
            int kolicina = int.Parse(txtKolicina.Text);

            
                    using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
                    {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO tblArtikliKuvar (NazivArtikla, Kolicina) VALUES (@nazivArtikla, @kolicina)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nazivArtikla", odabraniArtikal.NazivArtikla);
                    command.Parameters.AddWithValue("@kolicina", kolicina);
                    command.ExecuteNonQuery();

                    popupNaruciArtikal.IsOpen = false;
                    cmbArtikli.SelectedIndex = 0;
                    txtKolicina.Text = string.Empty;

                    MessageBox.Show("Uspešno ste poručili artikal.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom unosa podataka u bazu: " + ex.Message);
                }
            }
        }


        private void BtnOtkaziNaruci_Click(object sender, RoutedEventArgs e)
        {

            popupNaruciArtikal.IsOpen = false;


            cmbArtikli.SelectedIndex = 0;
            txtKolicina.Text = string.Empty;
        }
        private void btnHelp(object sender, RoutedEventArgs e)
        {
            string helpFilePath = "Res/kuhinja/Kuhinja2/Kuhinja2.html";
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
                            string tempHtmlFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Kuhinja2.html");
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
    }
}
