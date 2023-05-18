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
                        MessageBox.Show("Nemate pristup ovom sistemu.");
                        return; // Prekidamo dalje izvršavanje ako korisnik nema pristup
                    }

                    Close(); // Zatvaranje trenutnog prozora za logovanje
                }
                else
                {
                    MessageBox.Show("Korisničko ime ili lozinka su netačni.");
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

        private void Txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Provera kojem elementu je fokusiran taster Enter
                if (sender == txtUsername)
                {
                    txtPassword.Focus(); // Prebacivanje fokusa na polje za unos lozinke
                }
                else if (sender == txtPassword)
                {
                    BtnLogin(sender, e); // Izvršavanje logovanja ako je fokus na polju za unos lozinke
                }
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
