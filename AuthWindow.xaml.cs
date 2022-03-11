using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public static AuthWindow authWindow = new AuthWindow();
        bool isActivate = false;
        bool close = true;

        public AuthWindow()
        {
            InitializeComponent();
            setSavedSettings();

            if (auth_phone.Text != "")
            {
                auth_phone.IsEnabled = true;
                auth_password.IsEnabled = true;
                auth_verification_block.Visibility = Visibility.Hidden;
                error_msg.Visibility = Visibility.Hidden;
                success_msg.Visibility = Visibility.Hidden;

                bool autoauth = Properties.Settings.Default.autoauth;
                if(autoauth) login();
            }

            isActivate = false;
            close = true;
        }

        private void setSavedSettings()
        {
            try
            {
                auth_phone.Text = Properties.Settings.Default.mobilePhone;
                auth_password.Text = Properties.Settings.Default.password;
            }
            catch (Exception error)
            {

            }
        }

        string Config(string stepName)
        {
            string data = null;

            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    string phoneNumber = auth_phone.Text;
                    string code = auth_verification.Text;
                    string password = auth_password.Text;

                    if (stepName == "verification_code")
                    {
                        DispatcherTimer waitAuthCodeTimer = new DispatcherTimer();
                        waitAuthCodeTimer.Interval = TimeSpan.FromSeconds(2);
                        waitAuthCodeTimer.Tick += waitAuthCode_Timer;
                        waitAuthCodeTimer.Start();

                        async void waitAuthCode_Timer(object sender, EventArgs e)
                        {
                            auth_spinner.Visibility = Visibility.Hidden;
                            auth_verification_block.Visibility = Visibility.Visible;
                            auth_submit.Visibility = Visibility.Hidden;

                            waitAuthCodeTimer.Stop();
                        }
                    }

                    switch (stepName)
                    {
                        case "api_id": data = "740980"; break;
                        case "api_hash": data = "e5ec72f9394ae1d144b0c4b18abafc90"; break;
                        case "phone_number": data = phoneNumber; break;
                        case "verification_code": data = code; break;
                        case "first_name": data = ""; break;      // if sign-up is required
                        case "last_name": data = ""; break;       // if sign-up is required
                        case "password": data = password; break;
                        default: data = null; break;
                    }
                }catch(Exception error)
                {
                    isActivate = false;
                }
            });

            return data;
        }

        public static void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Properties.Settings.Default.lang)
            {
                case "UKR":
                    dict.Source = new Uri("..\\Resources\\UKR.xaml", UriKind.Relative);
                    break;
                case "ENG":
                    dict.Source = new Uri("..\\Resources\\ENG.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\RUS.xaml", UriKind.Relative);
                    break;
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
        private async Task login()
        {
            auth_spinner.Visibility = Visibility.Visible;

            isActivate = await App.createTGClient(Config);

            if (isActivate)
            {
                success_msg.Visibility = Visibility.Visible;

                MainWindow mainWindow = new MainWindow();
                DispatcherTimer waitTimer = new DispatcherTimer();

                waitTimer.Interval = TimeSpan.FromSeconds(2);
                waitTimer.Tick += wait_Timer;
                waitTimer.Start();

                void wait_Timer(object sender, EventArgs e)
                {
                    close = false;
                    this.Hide();
                    mainWindow.IsEnabled = true;
                    mainWindow.Show();
                    success_msg.Visibility = Visibility.Hidden;
                    waitTimer.Stop();
                    close = true;
                }
            }
            else
            {
                error_msg.Visibility = Visibility.Visible;
                auth_verification_block.Visibility = Visibility.Hidden;
                auth_spinner.Visibility = Visibility.Hidden;
                auth_submit.Visibility = Visibility.Visible;

                auth_phone.IsEnabled = true;
                auth_password.IsEnabled = true;
            }

            Properties.Settings.Default.mobilePhone = auth_phone.Text;
            Properties.Settings.Default.password = auth_password.Text;

            Properties.Settings.Default.Save();
        }
        private async void auth_submit_Click(object sender, RoutedEventArgs e)
        {
            await login();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(close) Environment.Exit(1);
        }

        private void auth_phone_LostFocus(object sender, RoutedEventArgs e)
        {
            string mobilePhone = auth_phone.Text;

            if(mobilePhone == "") {
                auth_verification_block.Visibility = Visibility.Hidden;
            }
        }

        private void auth_win_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                login();
            }
        }
    }
}
