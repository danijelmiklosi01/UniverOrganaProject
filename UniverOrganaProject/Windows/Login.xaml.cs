using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
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
using UniverOrganaProject.Windows;

namespace UniverOrganaProject
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

        private void BtnLogin(object sender, RoutedEventArgs e)
        {

            SqlConnection sqlCon = new SqlConnection(@"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True");
            try
            {
                if (sqlCon.State == System.Data.ConnectionState.Closed)
                    sqlCon.Open();
                String query = "SELECT RoleID FROM tblUser WHERE Username=@Username AND Password=@Password";
                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                sqlCmd.CommandType = System.Data.CommandType.Text;
                sqlCmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                sqlCmd.Parameters.AddWithValue("@Password", txtPassword.Password);
                object result = sqlCmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    int roleId = Convert.ToInt32(result);

                    if (roleId == 1)
                    {
                        Magacioner magacionerWindow = new Magacioner();
                        magacionerWindow.Show();
                    }
                    else if (roleId == 2)
                    {
                        Kuvar kuvarWindow = new Kuvar();
                        kuvarWindow.Show();
                    }
                    else
                    {
                        lblError.Content = "Nemate pristup ovom sistemu.";
                        return; // Prekidamo dalje izvršavanje ako korisnik nema pristup
                    }

                    Close(); // Zatvaranje trenutnog prozora za logovanje
                }
                else
                {
                    lblError.Content = "Korisničko ime ili lozinka su netačni.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sqlCon.Close();
            }
        }
        private void TxtUsername_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length >= 18)
            {
                e.Handled = true;
                lblError.Content = "Maksimalna dužina korisničkog imena je 18 karaktera.";
            }
            else
            {
                lblError.Content = string.Empty; // Reset error message
            }
        }

        private void TxtPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            if (passwordBox.Password.Length >= 18)
            {
                e.Handled = true;
                lblError.Content = "Maksimalna dužina šifre je 18 karaktera.";
            }
            else
            {
                lblError.Content = string.Empty; // Reset error message
            }
        }

        private void Txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // Stop further typing after pressing the Enter key
                                  // Check which element has focus when the Enter key is pressed
                if (sender == txtUsername)
                {
                    txtPassword.Focus(); // Move focus to the password input field
                }
                else if (sender == txtPassword)
                {
                    BtnLogin(sender, e); // Perform login if the focus is on the password input field
                }
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var email = "danijelmiklosi2001@gmail.com";
            var subject = "Zahtev za promenu lozinke";
            var body = $"Korisnik: {txtUsername.Text}{Environment.NewLine}Zahteva promenu lozinke.";

            var mailtoUri = new Uri($"mailto:{email}?subject={subject}&body={body}");

            try
            {
                Process.Start(new ProcessStartInfo(mailtoUri.AbsoluteUri));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nije moguće otvoriti email klijent.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
