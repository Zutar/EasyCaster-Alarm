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
        public AuthWindow()
        {
            InitializeComponent();

            if(auth_phone.Text != "")
            {
                auth_phone.IsEnabled = false;
                auth_password.IsEnabled = false;
                auth_verification_block.Visibility = Visibility.Hidden;

                auth_spinner.Visibility = Visibility.Visible;

                DispatcherTimer waitAuthTimer = new DispatcherTimer();
                waitAuthTimer.Interval = TimeSpan.FromSeconds(2);
                waitAuthTimer.Tick += waitAuth_Timer;
                waitAuthTimer.Start();

                async void waitAuth_Timer(object sender, EventArgs e)
                {
                    auth_phone.IsEnabled = true;
                    auth_password.IsEnabled = true;

                    auth_spinner.Visibility = Visibility.Hidden;

                    waitAuthTimer.Stop();
                }
            }
        }

        string Config(string stepName)
        {
            string data = null;

            this.Dispatcher.Invoke(() =>
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
            });

            return data;
        }

        private void auth_submit_Click(object sender, RoutedEventArgs e)
        {

            auth_spinner.Visibility = Visibility.Visible;

            App.createTGClient(Config);

            Properties.Settings.Default.mobilePhone = auth_phone.Text;
            Properties.Settings.Default.password = auth_password.Text;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(1);
        }

        private void auth_phone_LostFocus(object sender, RoutedEventArgs e)
        {
            string mobilePhone = auth_phone.Text;

            if(mobilePhone == "") {
                auth_verification_block.Visibility = Visibility.Hidden;
            }
        }
    }
}
