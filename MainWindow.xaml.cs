using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class LogItem
    {
        public string timestamp { get; set; }
        public string status { get; set; }
        public string color { get; set; }

        public LogItem(string timestamp, string status, string color)
        {
            this.timestamp = timestamp;
            this.status = status;
            this.color = color;
        }
    }

    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        bool settingsOpen = false;
        bool logsOpen = true;
        bool isEditSettings = false;
        bool isAlarm = false;

        ObservableCollection<LogItem> logs = new ObservableCollection<LogItem>();
        ObservableCollection<string> processNames = new ObservableCollection<string>();

        DispatcherTimer updateProcessListTimer = new DispatcherTimer();
        DispatcherTimer waitTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            SetLanguageDictionary();
            setSavedSettings();

            settings_block.Visibility = Visibility.Collapsed;
            logs_block.Visibility = Visibility.Visible;

            logs_list.ItemsSource = logs;
            action_app_list_1.ItemsSource = processNames;
            action_app_list_2.ItemsSource = processNames;
            action_app_list_3.ItemsSource = processNames;
            action_app_list_4.ItemsSource = processNames;
        }
        private void stopAll()
        {
            if(updateProcessListTimer != null) updateProcessListTimer.Stop();
            if (waitTimer != null) waitTimer.Stop();

            isEditSettings = false;
            isAlarm = false;

            disableAll();
        }

        private void pressKey(byte index)
        {
            string applicationName = "";
            string keyName = "";

            if(index == 1)
            {
                applicationName = (string)action_app_list_1.SelectedItem;
                keyName = action_key_press_1.Text;
            }
            else if(index == 2)
            {
                applicationName = (string)action_app_list_2.SelectedItem;
                keyName = action_key_press_2.Text;
            }
            else if (index == 3)
            {
                applicationName = (string)action_app_list_3.SelectedItem;
                keyName = action_key_press_3.Text;
            }
            else if (index == 4)
            {
                applicationName = (string)action_app_list_4.SelectedItem;
                keyName = action_key_press_4.Text;
            }

            Process p = Process.GetProcessesByName(applicationName).FirstOrDefault();
            if (p != null)
            {
                IntPtr h = p.MainWindowHandle;
                SetForegroundWindow(h);
                System.Windows.Forms.SendKeys.SendWait("{" + keyName + "}");
            }
        }

        private void addAlarmLog()
        {
            if (logs.Count == 15) logs.RemoveAt(0);

            if (isAlarm)
            {
                logs.Add(new LogItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), FindResource("alarmMessage").ToString(), "Red"));
            }
            else
            {
                logs.Add(new LogItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), FindResource("noAlarmMessage").ToString(), "Green"));
            }
        }

        private void startAlarm()
        {
            alarm_block.Visibility = Visibility.Visible;
            isAlarm = true;

            Storyboard blinkAnimation = TryFindResource("alarmBlinkAnimation") as Storyboard;
            if (blinkAnimation != null)
            {
                blinkAnimation.Begin();
            }

            addAlarmLog();
        }

        private void stopAlarm()
        {
            alarm_block.Visibility = Visibility.Collapsed;
            no_alarm_block.Visibility = Visibility.Visible;
            isAlarm = false;

            Storyboard alarmBlinkAnimation = TryFindResource("alarmBlinkAnimation") as Storyboard;
            if (alarmBlinkAnimation != null)
            {
                alarmBlinkAnimation.Stop();
            }

            Storyboard noAlarmBlinkAnimation = TryFindResource("noAlarmBlinkAnimation") as Storyboard;
            if (noAlarmBlinkAnimation != null)
            {
                noAlarmBlinkAnimation.Begin();
            }

            addAlarmLog();

            waitTimer.Interval = TimeSpan.FromSeconds(10);
            waitTimer.Tick += wait_Timer;
            waitTimer.Start();

            async void wait_Timer(object sender, EventArgs e)
            {
                no_alarm_block.Visibility = Visibility.Collapsed;
                noAlarmBlinkAnimation.Stop();
                waitTimer.Stop();
            }
        }

        private ObservableCollection<string> getActiveProcessNames()
        {
            ObservableCollection<string> processNames = new ObservableCollection<string> { };
            var processes = Process.GetProcesses();
            
            for(int i = 0; i < processes.Length; i++)
            {
                string processName = processes[i].ProcessName;
                if (processNames.Any(p => p == processName) == false) {
                    processNames.Add(processName);
                }
            }

            return processNames;
        }

        private void setSavedSettings()
        {
            try
            {
                tg_channel_main_name.Text = Properties.Settings.Default.tgMainChannelName;
                tg_channel_main_link.Text = Properties.Settings.Default.tgMainChannelLink;
                tg_channel_test_name.Text = Properties.Settings.Default.tgTestChannelName;
                tg_channel_test_link.Text = Properties.Settings.Default.tgTestChannelLink;

                app_autostart.IsChecked = Properties.Settings.Default.autostart;

                action_key_phrase_1.Text = Properties.Settings.Default.actionKeyPhrase1;
                action_key_phrase_2.Text = Properties.Settings.Default.actionKeyPhrase2;
                action_key_phrase_3.Text = Properties.Settings.Default.actionKeyPhrase3;
                action_key_phrase_4.Text = Properties.Settings.Default.actionKeyPhrase4;

                action_key_press_1.Text = Properties.Settings.Default.actionKeyPress1;
                action_key_press_2.Text = Properties.Settings.Default.actionKeyPress2;
                action_key_press_3.Text = Properties.Settings.Default.actionKeyPress3;
                action_key_press_4.Text = Properties.Settings.Default.actionKeyPress4;

                action_key_exception_1.Text = Properties.Settings.Default.actionKeyException1;
                action_key_exception_2.Text = Properties.Settings.Default.actionKeyException2;
                action_key_exception_3.Text = Properties.Settings.Default.actionKeyException3;
                action_key_exception_4.Text = Properties.Settings.Default.actionKeyException4;

                try
                {
                    action_app_list_1.SelectedItem = Properties.Settings.Default.actionAppList1;
                    action_app_list_2.SelectedItem = Properties.Settings.Default.actionAppList2;
                    action_app_list_3.SelectedItem = Properties.Settings.Default.actionAppList3;
                    action_app_list_4.SelectedItem = Properties.Settings.Default.actionAppList4;
                }
                catch(Exception e) { }

                processNames = getActiveProcessNames();

                updateProcessListTimer.Interval = TimeSpan.FromSeconds(10);
                updateProcessListTimer.Tick += updateProcessList_Timer;
                updateProcessListTimer.Start();

                async void updateProcessList_Timer(object sender, EventArgs e)
                {
                    processNames = getActiveProcessNames();
                }
            }catch(Exception error)
            {

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string answer = FindResource("closeAppAnswer").ToString();
            var Result = MessageBox.Show(answer, FindResource("closeAppTitle").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (Result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void settings_open_Click(object sender, RoutedEventArgs e)
        {
            if (settingsOpen)
            {
                settings_block.Visibility = Visibility.Collapsed;
                settings_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowDown.png"));
            }
            else
            {
                settings_block.Visibility = Visibility.Visible;
                settings_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowUp.png"));
            }

            settingsOpen = !settingsOpen;
        }

        private void SetLanguageDictionary()
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

        private void lang_rus_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "RUS";
            SetLanguageDictionary();
        }

        private void lang_ukr_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "UKR";
            SetLanguageDictionary();
        }

        private void m_about_Click(object sender, RoutedEventArgs e)
        {
            new About().ShowDialog();
        }

        private void m_feedback_Click(object sender, RoutedEventArgs e)
        {
            new Feedback().ShowDialog();
        }

        private void m_help_Click(object sender, RoutedEventArgs e)
        {
            new Help().ShowDialog();
        }

        private void app_autostart_Checked(object sender, RoutedEventArgs e)
        {
            bool autostart = (bool)app_autostart.IsChecked;

            if (autostart)
            {
                SetAutorunProgram(true);
            }
            else
            {
                SetAutorunProgram(false);
            }
        }

        public void SetAutorunProgram(bool autorun)
        {
            try
            {
                const string name = "EasyCaster Transcoder";

                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                        (@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (autorun)
                {
                    string programPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    registryKey.SetValue(name, programPath);
                }
                else
                {
                    registryKey.DeleteValue(name);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void settings_edit_Click(object sender, RoutedEventArgs e)
        {
            isEditSettings = true;

            tg_channel_main_name.IsEnabled = true;
            tg_channel_main_link.IsEnabled = true;
            tg_channel_test_name.IsEnabled = true;
            tg_channel_test_link.IsEnabled = true;

            app_autostart.IsEnabled = true;

            action_key_phrase_1.IsEnabled = true;
            action_key_phrase_2.IsEnabled = true;
            action_key_phrase_3.IsEnabled = true;
            action_key_phrase_4.IsEnabled = true;

            action_app_list_1.IsEnabled = true;
            action_app_list_2.IsEnabled = true;
            action_app_list_3.IsEnabled = true;
            action_app_list_4.IsEnabled = true;

            action_key_press_1.IsEnabled = true;
            action_key_press_2.IsEnabled = true;
            action_key_press_3.IsEnabled = true;
            action_key_press_4.IsEnabled = true;

            action_key_exception_1.IsEnabled = true;
            action_key_exception_2.IsEnabled = true;
            action_key_exception_3.IsEnabled = true;
            action_key_exception_4.IsEnabled = true;
        }
        private void disableAll()
        {
            isEditSettings = false;

            tg_channel_main_name.IsEnabled = false;
            tg_channel_main_link.IsEnabled = false;
            tg_channel_test_name.IsEnabled = false;
            tg_channel_test_link.IsEnabled = false;

            app_autostart.IsEnabled = false;

            action_key_phrase_1.IsEnabled = false;
            action_key_phrase_2.IsEnabled = false;
            action_key_phrase_3.IsEnabled = false;
            action_key_phrase_4.IsEnabled = false;

            action_app_list_1.IsEnabled = false;
            action_app_list_2.IsEnabled = false;
            action_app_list_3.IsEnabled = false;
            action_app_list_4.IsEnabled = false;

            action_key_press_1.IsEnabled = false;
            action_key_press_2.IsEnabled = false;
            action_key_press_3.IsEnabled = false;
            action_key_press_4.IsEnabled = false;

            action_key_exception_1.IsEnabled = false;
            action_key_exception_2.IsEnabled = false;
            action_key_exception_3.IsEnabled = false;
            action_key_exception_4.IsEnabled = false;
        }
        private void settings_save_Click(object sender, RoutedEventArgs e)
        {
            disableAll();
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.tgMainChannelName = tg_channel_main_name.Text;
            Properties.Settings.Default.tgMainChannelLink = tg_channel_main_link.Text;
            Properties.Settings.Default.tgTestChannelName = tg_channel_test_name.Text;
            Properties.Settings.Default.tgTestChannelLink = tg_channel_test_link.Text;
            Properties.Settings.Default.autostart = (bool)app_autostart.IsChecked;

            Properties.Settings.Default.actionKeyPhrase1 = action_key_phrase_1.Text;
            Properties.Settings.Default.actionKeyPhrase2 = action_key_phrase_2.Text;
            Properties.Settings.Default.actionKeyPhrase3 = action_key_phrase_3.Text;
            Properties.Settings.Default.actionKeyPhrase4 = action_key_phrase_4.Text;

            Properties.Settings.Default.actionAppList1 = action_app_list_1.Text;
            Properties.Settings.Default.actionAppList2 = action_app_list_2.Text;
            Properties.Settings.Default.actionAppList3 = action_app_list_3.Text;
            Properties.Settings.Default.actionAppList4 = action_app_list_4.Text;

            Properties.Settings.Default.actionKeyPress1 = action_key_press_1.Text;
            Properties.Settings.Default.actionKeyPress2 = action_key_press_2.Text;
            Properties.Settings.Default.actionKeyPress3 = action_key_press_3.Text;
            Properties.Settings.Default.actionKeyPress4 = action_key_press_4.Text;

            Properties.Settings.Default.actionKeyException1 = action_key_exception_1.Text;
            Properties.Settings.Default.actionKeyException2 = action_key_exception_2.Text;
            Properties.Settings.Default.actionKeyException3 = action_key_exception_3.Text;
            Properties.Settings.Default.actionKeyException4 = action_key_exception_4.Text;

            Properties.Settings.Default.Save();
        }

        private void settings_save_config_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                string data = "";

                StreamReader sr = new StreamReader(path);
                data = sr.ReadToEnd();
                sr.Close();

                if (data != "")
                {

                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Title = FindResource("saveFileDialog").ToString();
                    saveFileDialog1.FileName = Path.GetFileName("user.config");

                    if (saveFileDialog1.ShowDialog() == true)
                    {
                        string filename = saveFileDialog1.FileName;
                        File.WriteAllText(filename, data);
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void settings_load_config_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                string data = "";

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "config files (*.config)|*.config";

                if (ofd.ShowDialog() == true)
                {
                    StreamReader sr = new StreamReader(ofd.FileName);
                    data = sr.ReadToEnd();
                    sr.Close();
                }

                if (data != "")
                {
                    FileInfo file = new FileInfo(path);
                    file.Directory.Create();
                    File.WriteAllText(path, data);
                }

                MessageBox.Show(FindResource("settingsSaveAlert").ToString());
            }
            catch (Exception error)
            {

            }
        }

        private void logs_open_Click(object sender, RoutedEventArgs e)
        {
            if (logsOpen)
            {
                logs_block.Visibility = Visibility.Collapsed;
                logs_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowDown.png"));
            }
            else
            {
                logs_block.Visibility = Visibility.Visible;
                logs_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowUp.png"));
            }

            logsOpen = !logsOpen;
        }

        private void action_key_test_1_Click(object sender, RoutedEventArgs e)
        {
            pressKey(1);
        }

        private void action_key_test_2_Click(object sender, RoutedEventArgs e)
        {
            pressKey(2);
        }

        private void action_key_test_3_Click(object sender, RoutedEventArgs e)
        {
            pressKey(3);
        }

        private void action_key_test_4_Click(object sender, RoutedEventArgs e)
        {
            pressKey(4);
        }

        private void action_key_press_1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            action_key_press_1.Text = e.Key.ToString();
        }

        private void action_key_press_2_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            action_key_press_2.Text = e.Key.ToString();
        }

        private void action_key_press_3_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            action_key_press_3.Text = e.Key.ToString();
        }

        private void action_key_press_4_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            action_key_press_4.Text = e.Key.ToString();
        }

        private void app_logout_Click(object sender, RoutedEventArgs e)
        {
            stopAll();

            App.authWindow.ShowDialog();
            app.IsEnabled = false;
        }
    }
}
