using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TL;
using WTelegram;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string errorMessage = "An unhandled exception occurred";
        public static Client client = null;
        public static AuthWindow authWindow = new AuthWindow();

        public App()
        {
            bool isOnlyOneSameProgram = checkForSameApp();
            if (isOnlyOneSameProgram)
            {
                MessageBox.Show("Програма вже запущена. Будь ласка перевірте диспетчер завдань, можливо у фоновому режимі працює інший екземпляр програми, запущений раніше", "Увага", MessageBoxButton.OK);
                Environment.Exit(0);
            }

            AuthWindow.SetLanguageDictionary();

            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        private bool checkForSameApp()
        {
            bool onlyInstance;
            new Mutex(true, "EasyCaster Alarm", out onlyInstance);
            var exists = false;
            Process[] processes = Process.GetProcessesByName("EasyCaster Alarm");
            if (processes.Length > 1)
            {
                exists = true;
            }

            if (!onlyInstance || exists)
            {
                return true;
            }

            return false;
        }

        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                errorMessage = e.Exception.ToString();

                if (errorMessage.IndexOf("USER_DEACTIVATED_BAN") != -1)
                {
                    MessageBox.Show("Наданий номер телефону заблоковано в Telegram. Для відновлення напишіть тех. підтримці telegram (recover@telegram.org). У повідомленні має бути ваш номер телефону, який прив'язаний до Telegram");
                }else if (errorMessage.IndexOf("because it is being used by another process") != -1)
                {
                    MessageBox.Show("Програма вже запущена.Будь ласка перевірте диспетчер завдань, можливо у фоновому режимі працює інший екземпляр програми, запущений раніше");
                }
                else
                {
                    ExceptionWindow exceptionWindow = new ExceptionWindow();
                    exceptionWindow.ShowDialog();
                }
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
                client.MaxAutoReconnects = 10;

                return true;
            }catch(Exception error)
            {
                string errorMessage = error.ToString();
                if (errorMessage.IndexOf("PHONE_NUMBER_INVALID") != -1)
                {
                    MessageBox.Show("Неправильний або недійсний номер телефону");
                }
                else if (errorMessage.IndexOf("PHONE_NUMBER_BANNED") != -1)
                {
                    MessageBox.Show("Наданий номер телефону заблоковано в Telegram. Для відновлення напишіть тех. підтримці telegram (recover@telegram.org). У повідомленні має бути ваш номер телефону, який прив'язаний до Telegram");
                }
                else if (errorMessage.IndexOf("PHONE_CODE_EXPIRED") != -1)
                {
                    MessageBox.Show("Термін дії наданого вами телефонного коду закінчився");
                }
                else if (errorMessage.IndexOf("PHONE_CODE_INVALID") != -1)
                {
                    MessageBox.Show("Наданий телефонний код недійсний");
                }
                else if (errorMessage.IndexOf("PHONE_NUMBER_UNOCCUPIED") != -1)
                {
                    MessageBox.Show("Номер телефону ще не використовується. Спочатку потрібно зареєструвати обліковий запис Telegram");
                }
                else if (errorMessage.IndexOf("API_ID_INVALID") != -1)
                {
                    MessageBox.Show("Комбінація api_id/api_hash недійсна. Зв'яжіться з техпідтримкою live-tv service (livetv-help@ukr.net)");
                }
                else if (errorMessage.IndexOf("FLOOD_WAIT") != -1)
                {
                    MessageBox.Show("Перевищено обмеження Telegram API. Спробуйте трохи пізніше");
                }
                else if (errorMessage.IndexOf("PASSWORD_HASH_INVALID") != -1)
                {
                    MessageBox.Show("Наданий хеш пароля недійсний");
                }
                else {
                    MessageBox.Show(error.ToString());
                }
                return false;
            }
        }

        public static void logout()
        {
            try
            {
                if (client != null)
                {
                    client.Auth_LogOut();
                    client.Dispose();
                    client = null;
                }
            }catch(Exception error) { }
        }

        private static string getOSversion()
        {
            string r = "не определено";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                ManagementObjectCollection information = searcher.Get();
                if (information != null)
                {
                    foreach (ManagementObject obj in information)
                    {
                        r = obj["Caption"].ToString() + " - " + obj["OSArchitecture"].ToString();
                    }
                }
                r = r.Replace("NT 5.1.2600", "XP");
                r = r.Replace("NT 5.2.3790", "Server 2003");

                return r;
            }
        }

        private static bool isActiveNetwork()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }
        private static string getMyPublicIP()
        {
            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return IPAddress.Parse(externalIpString).ToString();
        }

        public static void sendEmailMessage(string subject, string body, bool addAdditionalInfo = true)
        {
            var smtpClient = new SmtpClient("in-v3.mailjet.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("58b452caf2ac4342579708f4910d5fe1", "2ec0f941a5d4832a0cb00f4815fc4d3a"),
                EnableSsl = true,
                // Credentials = new NetworkCredential("livetvhelpservice@gmail.com", "livetvhelpservice575797"),
            };

            if (addAdditionalInfo)
            {
                body += "<br><br><br><span><b>Система: </b>" + getOSversion() + "</span><br>";
                body += "<span><b>Сеть: </b>" + (isActiveNetwork() ? "есть" : "нет") + "</span><br>";
                body += "<span><b>Внешний IP адрес: </b>" + getMyPublicIP() + "</span><br>";
            }

            MailMessage message = new MailMessage("livetv-help@ukr.net", "livetv-help@ukr.net");
            try
            {
                message.Subject = subject;
                message.Body = body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;

                smtpClient.Send(message);
            }catch(Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
    }
}
