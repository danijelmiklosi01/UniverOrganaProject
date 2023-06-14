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
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using UniverOrganaProject.Modeli;
using System.Printing;

namespace UniverOrganaProject.Windows
{
    /// <summary>
    /// Interaction logic for Magacioner.xaml
    /// </summary>
    public partial class Magacioner : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Artikli> listaArtikala;
        private string korisnickoIme;
        private Stack<ObservableCollection<Artikli>> undoStack;
        private Stack<ObservableCollection<Artikli>> redoStack;



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

        public Magacioner(string korisnickoIme)
        {
            InitializeComponent();
            this.korisnickoIme = korisnickoIme;
            DataContext = this;
            LoadDataFromDatabase();
            undoStack = new Stack<ObservableCollection<Artikli>>();
            redoStack = new Stack<ObservableCollection<Artikli>>();
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
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btnHelp(object sender, RoutedEventArgs e)
        {
            string helpFilePath = "Res/magacionerHelp/magacionerHelp.html";
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
        private void Undo()
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(new ObservableCollection<Artikli>(ListaArtikala));
                ListaArtikala = undoStack.Pop();
                OnPropertyChanged(nameof(ListaArtikala));

                UpdateDatabaseWithArtikli(ListaArtikala);
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(new ObservableCollection<Artikli>(ListaArtikala));
                ListaArtikala = redoStack.Pop();
                OnPropertyChanged(nameof(ListaArtikala));

                UpdateDatabaseWithArtikli(ListaArtikala);
            }
        }

        private void UpdateDatabaseWithArtikli(ObservableCollection<Artikli> artikli)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                
                SqlCommand deleteCommand = new SqlCommand("DELETE FROM tblArtikli", connection);
                deleteCommand.ExecuteNonQuery();

                
                foreach (Artikli artikal in artikli)
                {
                    SqlCommand insertCommand = new SqlCommand("INSERT INTO tblArtikli (nazivArtikla, rokTrajanja, Kolicina) VALUES (@nazivArtikla, @rokTrajanja, @kolicina)", connection);
                    insertCommand.Parameters.AddWithValue("@nazivArtikla", artikal.NazivArtikla);
                    insertCommand.Parameters.AddWithValue("@rokTrajanja", artikal.RokTrajanja);
                    insertCommand.Parameters.AddWithValue("@kolicina", artikal.Kolicina);

                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        private void btnUndo(object sender, RoutedEventArgs e)
        {
            Undo();
            lstArtikli.ItemsSource = ListaArtikala;
        }
        private void btnRedo(object sender, RoutedEventArgs e)
        {
            Redo();
            lstArtikli.ItemsSource = ListaArtikala;
        }
        private void btnDodaj(object sender, RoutedEventArgs e)
        {
            popupDodajArtikal.IsOpen = true;
        }
        private void btnOduzmi(object sender, RoutedEventArgs e)
        {
            popupOduzmiArtikal.IsOpen = true;
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


        private void btnList(object sender, RoutedEventArgs e)
        {
            MagacionerNarudzbe magacionerNarudzbe = new MagacionerNarudzbe(korisnickoIme);
            magacionerNarudzbe.Show();
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
        
            private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = txtSearch.Text;
            List<Artikli> filteredArtikli = ListaArtikala.Where(a => a.NazivArtikla.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            lstArtikli.ItemsSource = filteredArtikli;
        }
        private void BtnDodajPopup_Click(object sender, RoutedEventArgs e)
        {
            undoStack.Push(new ObservableCollection<Artikli>(ListaArtikala));
            redoStack.Clear();
            string nazivArtikla = txtNazivArtiklaPopup.Text;
            DateTime? rokTrajanja = datePickerRokTrajanjaPopup.SelectedDate;
            string kolicinaText = txtKolicinaPopup.Text;

            if (string.IsNullOrEmpty(nazivArtikla) || !rokTrajanja.HasValue || string.IsNullOrEmpty(kolicinaText))
            {
                MessageBox.Show("Molimo popunite sva polja.");
                return;
            }

            if (!int.TryParse(kolicinaText, out int kolicina) || kolicina <= 0)
            {
                MessageBox.Show("Unesite ispravnu količinu.");
                return;
            }

            Artikli noviArtikal = new Artikli
            {
                NazivArtikla = nazivArtikla,
                RokTrajanja = rokTrajanja.Value,
                Kolicina = kolicina
            };

            ListaArtikala.Add(noviArtikal);

            popupDodajArtikal.IsOpen = false;

            AddArtikalToDatabase(noviArtikal);

            lstArtikli.ItemsSource = ListaArtikala;

            txtNazivArtiklaPopup.Text = string.Empty;
            datePickerRokTrajanjaPopup.SelectedDate = null;
            txtKolicinaPopup.Text = string.Empty;
            OnPropertyChanged(nameof(ListaArtikala));
        }

        private void AddArtikalToDatabase(Artikli artikal)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("INSERT INTO tblArtikli (nazivArtikla, rokTrajanja, Kolicina) VALUES (@nazivArtikla, @rokTrajanja, @kolicina)", connection);
                command.Parameters.AddWithValue("@nazivArtikla", artikal.NazivArtikla);
                command.Parameters.AddWithValue("@rokTrajanja", artikal.RokTrajanja);
                command.Parameters.AddWithValue("@kolicina", artikal.Kolicina);

                command.ExecuteNonQuery();
            }
        }
        private void BtnOtkaziPopup_Click(object sender, RoutedEventArgs e)
        {
            
            popupDodajArtikal.IsOpen = false;

            
            txtNazivArtiklaPopup.Text = string.Empty;
            datePickerRokTrajanjaPopup.SelectedDate = null;
            txtKolicinaPopup.Text = string.Empty;
        }

        private void BtnOduzmiPopup_Click(object sender, RoutedEventArgs e)
        {
            undoStack.Push(new ObservableCollection<Artikli>(ListaArtikala));
            redoStack.Clear();
            if (cmbArtikli.SelectedItem is Artikli selectedArtikal)
            {
                if (int.TryParse(txtKolicina.Text, out int kolicina))
                {
                    if (selectedArtikal.Kolicina >= kolicina)
                    {
                        selectedArtikal.Kolicina -= kolicina;
                        SaveArtikalToDatabase(selectedArtikal);
                    }
                    else
                    {
                        MessageBox.Show("Nemate dovoljno artikala za oduzimanje.");
                    }
                }
                else
                {
                    MessageBox.Show("Unesite ispravnu količinu.");
                }
                lstArtikli.ItemsSource = ListaArtikala;
            }

            
            popupOduzmiArtikal.IsOpen = false;
            OnPropertyChanged(nameof(ListaArtikala));
        }

        private void BtnOtkaziOduzmi_Click(object sender, RoutedEventArgs e)
        {
           
            popupOduzmiArtikal.IsOpen = false;

            
            cmbArtikli.SelectedIndex = 0;
            txtKolicina.Text = string.Empty;
        }


        private void SaveArtikalToDatabase(Artikli artikal)
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True"))
            {
                connection.Open();

                if (artikal.Kolicina == 0)
                {
                    SqlCommand deleteCommand = new SqlCommand("DELETE FROM tblArtikli WHERE NazivArtikla = @nazivArtikla AND RokTrajanja = @rokTrajanja", connection);
                    deleteCommand.Parameters.AddWithValue("@nazivArtikla", artikal.NazivArtikla);
                    deleteCommand.Parameters.AddWithValue("@rokTrajanja", artikal.RokTrajanja);
                    deleteCommand.ExecuteNonQuery();
                }
                else
                {
                    SqlCommand updateCommand = new SqlCommand("UPDATE tblArtikli SET Kolicina = @kolicina WHERE NazivArtikla = @nazivArtikla AND RokTrajanja = @rokTrajanja", connection);
                    updateCommand.Parameters.AddWithValue("@kolicina", artikal.Kolicina);
                    updateCommand.Parameters.AddWithValue("@nazivArtikla", artikal.NazivArtikla);
                    updateCommand.Parameters.AddWithValue("@rokTrajanja", artikal.RokTrajanja);
                    updateCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Artikal je uspešno oduzet.");

                LoadDataFromDatabase();

                popupOduzmiArtikal.IsOpen = false;
            }
        }

    }
}
