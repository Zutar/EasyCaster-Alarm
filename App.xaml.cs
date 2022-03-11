using System;
using System.Threading.Tasks;
using System.Windows;
using WTelegram;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Client client;
        public static AuthWindow authWindow = new AuthWindow();

        public App()
        {
            AuthWindow.SetLanguageDictionary();

            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            //authWindow.ShowDialog();
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.ToString());
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            catch (Exception error)
            {

            }
        }

        public static async Task<bool> createTGClient(Func<string, string> configProvider = null)
        {
            client = new Client(configProvider);

            try
            {
                var my = await client.LoginUserIfNeeded();
                App.client.MaxAutoReconnects = 10;
                //MessageBox.Show($"We are logged-in as {my.username ?? my.first_name + " " + my.last_name} (id {my.id})");

                return true;
            }catch(Exception error)
            {
                MessageBox.Show(error.ToString());
                return false;
            }
        }
    }
}
