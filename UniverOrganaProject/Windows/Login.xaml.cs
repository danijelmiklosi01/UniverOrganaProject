using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using UniverOrganaProject.Windows;
using Path = System.IO.Path;

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
                lblError.Content = string.Empty;
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
                lblError.Content = string.Empty;
            }
        }

        private void Txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (sender == txtUsername)
                {
                    txtPassword.Focus();
                }
                else if (sender == txtPassword)
                {
                    BtnLogin(sender, e);
                }
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string connectionString = @"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True";
            string email;

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    if (sqlCon.State == System.Data.ConnectionState.Closed)
                        sqlCon.Open();

                    string query = "SELECT Email FROM tblUser WHERE Username=@Username";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@Username", txtUsername.Text);

                    using (SqlDataReader reader = sqlCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            email = reader["Email"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Korisničko ime nije pronađeno.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom izvršavanja upita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Generisanje nove lozinke
            string newPassword = GenerateNewPassword();

            // Ažuriranje lozinke u bazi podataka
            if (!UpdatePasswordInDatabase(txtUsername.Text, newPassword))
            {
                MessageBox.Show("Došlo je do greške prilikom ažuriranja lozinke.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Slanje e-pošte s novom lozinkom
            if (!SendEmail(email, newPassword))
            {
                MessageBox.Show("Došlo je do greške prilikom slanja e-pošte.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Nova lozinka je poslata na vašu e-adresu.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GenerateNewPassword()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            string newPassword = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

            return newPassword;
        }

        private bool UpdatePasswordInDatabase(string username, string newPassword)
        {
            string connectionString = @"Data Source=DANIJEL; Initial Catalog=UniverOrgana; Integrated Security=True";

            using (SqlConnection sqlCon = new SqlConnection(connectionString))
            {
                try
                {
                    if (sqlCon.State == System.Data.ConnectionState.Closed)
                        sqlCon.Open();

                    string query = "UPDATE tblUser SET Password=@Password WHERE Username=@Username";
                    SqlCommand sqlCmd = new SqlCommand(query, sqlCon);
                    sqlCmd.Parameters.AddWithValue("@Password", newPassword);
                    sqlCmd.Parameters.AddWithValue("@Username", username);
                    int rowsAffected = sqlCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom ažuriranja lozinke u bazi podataka: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private bool SendEmail(string recipientEmail, string newPassword)
        {
            try
            {
                string senderEmail = "smtpdanijel@gmail.com";
                string senderPassword = "leisiujlmpkdacbe";

                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Timeout = 10000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);

                    MailMessage mail = new MailMessage(senderEmail, recipientEmail);
                    mail.Subject = "Promena lozinke";
                    mail.Body = $"Vaša nova lozinka je: {newPassword}";

                    client.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Došlo je do greške prilikom slanja e-pošte: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
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
        private void btnHelp(object sender, RoutedEventArgs e)
        {
            string helpFilePath = "Res/loginHelp/loginHelp.html";
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
                            string tempHtmlFilePath = Path.Combine(Path.GetTempPath(), "loginHelp.html");
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
