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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TL;
using Brush = System.Windows.Media.Brush;
using WindowsInput;
using WindowsInput.Native;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using EasyCaster_Alarm.classes;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class LogItem
    {
        public string timestamp { get; set; }
        public string status { get; set; }

        public LogItem(string timestamp, string status)
        {
            this.timestamp = timestamp;
            this.status = status;
        }
    }

    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        bool settingsOpen = false;
        bool logsOpen = true;
        bool isAlarm = false;

        Storyboard sb;

        ObservableCollection<LogItem> logs = new ObservableCollection<LogItem>();
        ObservableCollection<string> processAppNames = new ObservableCollection<string>{ };
        ObservableCollection<ScheduleItem> scheduleItems = new ObservableCollection<ScheduleItem>();

        DispatcherTimer updateProcessListTimer = new DispatcherTimer();
        DispatcherTimer waitTimer = new DispatcherTimer();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer ticker_timer = new DispatcherTimer();
        DispatcherTimer config_timer = new DispatcherTimer();

        int keyWinCode1;
        int keyWinCode2;
        int keyWinCode3;
        int keyWinCode4;
        int keyWinCode5;
        int keyWinCode6;
        int keyWinCode7;
        int keyWinCode8;
        int schedulerKeyWinCode;
        int link_id = 0;
        int link_count = 0;

        string[] config_url = { "http://live-tv.od.ua/easycaster/alarm.json", "http://easycaster.tv/conf/alarm.json", "http://easycaster.net/conf/alarm.json" };
        JObject program_config;

        private static readonly HttpClient _httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            setSavedSettings();

            bool autostart = (bool)app_autostart.IsChecked;
            SetAutorunProgram(autostart);

            DispatcherTimer startTimer = new DispatcherTimer();
            startTimer.Interval = TimeSpan.FromMilliseconds(Convert.ToInt64(start_delay.Text));
            startTimer.Tick += start_Timer;
            startTimer.Start();

            void start_Timer(object sender, EventArgs e)
            {
                app_logout.IsEnabled = true;
                start();
                startTimer.Stop();
            }
        }
        
        private async void start()
        {
            App.client.OnUpdate += Client_Update;

            GetProgramConfig();
            getActiveProcessNames();

            settings_block.Visibility = Visibility.Collapsed;
            logs_block.Visibility = Visibility.Visible;

            logs_list.ItemsSource = logs;
            scheduler_list.ItemsSource = scheduleItems;

            updateProcessListTimer.Interval = TimeSpan.FromSeconds(30);
            updateProcessListTimer.Tick += updateProcessList_Timer;
            updateProcessListTimer.Start();

            ticker_timer.Tick += new EventHandler(LinkTimer_Tick);
            ticker_timer.IsEnabled = false;

            config_timer.Tick += new EventHandler(Config_update_timer_Tick);
            config_timer.Interval = new TimeSpan(1, 0, 0);
            config_timer.IsEnabled = true;

            void updateProcessList_Timer(object sender, EventArgs e)
            {
                getActiveProcessNames();
            }

            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += timer_Tick;
            timer.Start();

            async void timer_Tick(object sender, EventArgs e)
            {
                BrushConverter bc = new BrushConverter();

                try
                {
                    bool connectionStatus = !App.client.Disconnected;
                    
                    if (connectionStatus)
                    {
                        status.Background = (Brush)bc.ConvertFrom("#FF2FC300");
                    }
                    else
                    {
                        status.Background = (Brush)bc.ConvertFrom("#FFFF0000");
                        App.client.Reset(false, true);
                        await App.client.LoginUserIfNeeded();

                    }
                }catch(Exception error)
                {
                    status.Background = (Brush)bc.ConvertFrom("#FF2FC300");
                }
            }
        }

        private void stopAll()
        {
            if(updateProcessListTimer != null) updateProcessListTimer.Stop();
            if (waitTimer != null) waitTimer.Stop();
            if (timer != null) timer.Stop();
            if (ticker_timer != null) ticker_timer.Stop();
            if (config_timer != null) config_timer.Stop();

            isAlarm = false;
            App.logout();

            disableAll();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch (Exception) { }
        }

        private void pressKey(byte index)
        {
            try
            {
                string applicationName = "";

                int keyWinCode = 0;

                if (index == 1)
                {
                    applicationName = processAppNames[action_app_list_1.SelectedIndex];
                    keyWinCode = keyWinCode1;
                }
                else if (index == 2)
                {
                    applicationName = processAppNames[action_app_list_2.SelectedIndex];
                    keyWinCode = keyWinCode2;
                }
                else if (index == 3)
                {
                    applicationName = processAppNames[action_app_list_3.SelectedIndex];
                    keyWinCode = keyWinCode3;
                }
                else if (index == 4)
                {
                    applicationName = processAppNames[action_app_list_4.SelectedIndex];
                    keyWinCode = keyWinCode4;
                }
                else if (index == 5)
                {
                    applicationName = processAppNames[action_app_list_5.SelectedIndex];
                    keyWinCode = keyWinCode5;
                }
                else if (index == 6)
                {
                    applicationName = processAppNames[action_app_list_6.SelectedIndex];
                    keyWinCode = keyWinCode6;
                }
                else if (index == 7)
                {
                    applicationName = processAppNames[action_app_list_7.SelectedIndex];
                    keyWinCode = keyWinCode7;
                }
                else if (index == 8)
                {
                    applicationName = processAppNames[action_app_list_8.SelectedIndex];
                    keyWinCode = keyWinCode8;
                }


                Process p = Process.GetProcessesByName(applicationName).FirstOrDefault();
                Process currentProcess = Process.GetCurrentProcess();

                if (currentProcess != null && p != null)
                {
                    IntPtr h = p.MainWindowHandle;

                    DispatcherTimer appTimer = new DispatcherTimer();
                    appTimer.Interval = TimeSpan.FromMilliseconds(500);
                    appTimer.Tick += app_Timer;
                    appTimer.Start();

                    void app_Timer(object sender, EventArgs e)
                    {
                        ShowWindow(h, 9);
                        SetWindowPos(h, HWND_TOP, 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);
                        SwitchToThisWindow(h, true);

                        InputSimulator s = new InputSimulator();
                        s.Keyboard.KeyPress((VirtualKeyCode)keyWinCode);

                        appTimer.Stop();
                    }
                }
            }catch(Exception error) { }
        }

        public static async Task<string> Get(string url)
        {
            using (var result = await _httpClient.GetAsync(url))
            {
                string content = await result.Content.ReadAsStringAsync();
                return content;
            }
        }

        public static async void Post(string url, JObject data)
        {
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(url, content);
        }

        private void LinkTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (link_id >= link_count)
                {
                    link_id = 0;
                }
                string background, color;

                background = program_config["ticker"][link_id]["background"].ToString();
                color = program_config["ticker"][link_id]["textcolor"].ToString();

                background = background != "" ? background : "#FFC9CFD6";
                color = color != "" ? color : "#000000";

                string text = program_config["ticker"][link_id]["text"].ToString();
                int interval = (int)program_config["ticker"][link_id]["timer"];

                BrushConverter bc = new BrushConverter();
                ticker.Content = text;
                try
                {
                    ticker_link.NavigateUri = new Uri(program_config["ticker"][link_id]["href"].ToString());
                }
                catch (Exception) { }
                ticker.Background = (Brush)bc.ConvertFrom(background);
                ticker.Foreground = (Brush)bc.ConvertFrom(color);
                ticker_timer.Interval = new TimeSpan(0, 0, 0, 0, interval);
                link_id++;
            }
            catch (Exception error)
            {

            }
        }

        public async void GetProgramConfig()
        {
            for (int i = 0; i < config_url.Length; i++)
            {
                try
                {
                    program_config = JObject.Parse(await Get(config_url[i]));
                }
                catch (Exception)
                {
                    continue;
                }

                if (program_config.Count > 0)
                {
                    link_id = 0;
                    link_count = program_config["ticker"].Count();


                    if (link_count > 0)
                    {
                        try
                        {
                            string bgcolor = program_config["ticker"][link_id]["background"].ToString();
                            string textcolor = program_config["ticker"][link_id]["textcolor"].ToString();
                            string text = program_config["ticker"][link_id]["text"].ToString();

                            ticker_timer.IsEnabled = true;
                            ticker_timer.Interval = new TimeSpan(0, 0, 0, 0, (int)program_config["ticker"][link_id]["timer"]);

                            ticker.Visibility = Visibility.Visible;
                            ticker.Content = text;
                            try
                            {
                                ticker_link.NavigateUri = new Uri(program_config["ticker"][link_id]["href"].ToString());
                            }
                            catch (Exception) { }
                            BrushConverter bc = new BrushConverter();
                            ticker.Background = (Brush)bc.ConvertFrom(bgcolor);
                            ticker.Foreground = (Brush)bc.ConvertFrom(textcolor);
                        }catch(Exception e) { }
                        link_id++;
                    }
                    else
                    {
                        ticker_timer.IsEnabled = false;
                        ticker.Visibility = Visibility.Hidden;
                    }

                    break;
                }
            }
        }

        private void Config_update_timer_Tick(object sender, EventArgs e)
        {
            GetProgramConfig();
        }

        private void addAlarmLog(string message)
        {
            if (logs.Count == 15) logs.RemoveAt(0);

            if (isAlarm)
            {
                logs.Insert(0, new LogItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
            else
            {
                logs.Insert(0, new LogItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message));
            }
        }

        private void doAlarm(string message, byte index, string targetMessage)
        {
            alarm_block.Visibility = Visibility.Visible;
            isAlarm = true;

            Storyboard alarmBlinkAnimation = TryFindResource("alarmBlinkAnimation") as Storyboard;
            if (alarmBlinkAnimation != null)
            {
                alarmBlinkAnimation.Begin();
            }

            addAlarmLog(message);
            writeToFile(targetMessage, index);
            pressKey(index);
            startExtApp(index);
            updateSchedulers(index);

            if (webhook_url_1.Text != "") sendWebhook(webhook_url_1.Text, message, index, targetMessage);

            waitTimer.Interval = TimeSpan.FromSeconds(10);
            waitTimer.Tick += wait_Timer;
            waitTimer.Start();

            async void wait_Timer(object sender, EventArgs e)
            {
                alarm_block.Visibility = Visibility.Hidden;
                alarmBlinkAnimation.Stop();
                waitTimer.Stop();
            }
        }

        private void sendWebhook(string url = "", string message = "Test message", byte index = 0, string targetMessage = "Test")
        {
            try
            {
                JObject data = new JObject();
                data.Add("id", index);
                data.Add("message", message);
                data.Add("targetMessage", targetMessage);
                data.Add("dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                Post(webhook_url_1.Text, data);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void startExtApp( byte index)
        {
            try
            {
                string applicationPath = "";

                if (index == 1)
                {
                    applicationPath = action_key_ext_app_1.Text;
                }
                else if (index == 2)
                {
                    applicationPath = action_key_ext_app_2.Text;
                }
                else if (index == 3)
                {
                    applicationPath = action_key_ext_app_3.Text;
                }
                else if (index == 4)
                {
                    applicationPath = action_key_ext_app_4.Text;
                }
                else if (index == 5)
                {
                    applicationPath = action_key_ext_app_5.Text;
                }
                else if (index == 6)
                {
                    applicationPath = action_key_ext_app_6.Text;
                }
                else if (index == 7)
                {
                    applicationPath = action_key_ext_app_7.Text;
                }
                else if (index == 8)
                {
                    applicationPath = action_key_ext_app_8.Text;
                }

                Process.Start(applicationPath);
            }
            catch(Exception error)
            {

            }
        }

        private void writeToFile(string message, byte index)
        {
            string path = messageFolder.Text;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllText(path + "\\" + index + ".txt", message);
        }

        private void getActiveProcessNames()
        {
            processAppNames.Clear();

            var processes = Process.GetProcesses();

            action_app_list_1.Items.Clear();
            action_app_list_2.Items.Clear();
            action_app_list_3.Items.Clear();
            action_app_list_4.Items.Clear();
            action_app_list_5.Items.Clear();
            action_app_list_6.Items.Clear();
            action_app_list_7.Items.Clear();
            action_app_list_8.Items.Clear();

            scheduler_action_app_list.Items.Clear();

            foreach (Process p in processes)
            {
                if (!String.IsNullOrEmpty(p.MainWindowTitle))
                {
                    string processName = p.ProcessName;
                    string processTitle = p.MainWindowTitle;

                    processAppNames.Add(processName);

                    action_app_list_1.Items.Add(processTitle);
                    action_app_list_2.Items.Add(processTitle);
                    action_app_list_3.Items.Add(processTitle);
                    action_app_list_4.Items.Add(processTitle);
                    action_app_list_5.Items.Add(processTitle);
                    action_app_list_6.Items.Add(processTitle);
                    action_app_list_7.Items.Add(processTitle);
                    action_app_list_8.Items.Add(processTitle);

                    scheduler_action_app_list.Items.Add(processTitle);
                }
            }

            action_app_list_1.Items.Refresh();
            action_app_list_2.Items.Refresh();
            action_app_list_3.Items.Refresh();
            action_app_list_4.Items.Refresh();
            action_app_list_5.Items.Refresh();
            action_app_list_6.Items.Refresh();
            action_app_list_7.Items.Refresh();
            action_app_list_8.Items.Refresh();

            scheduler_action_app_list.Items.Refresh();

            try
            {
                action_app_list_1.SelectedItem = Properties.Settings.Default.actionAppList1;
                action_app_list_2.SelectedItem = Properties.Settings.Default.actionAppList2;
                action_app_list_3.SelectedItem = Properties.Settings.Default.actionAppList3;
                action_app_list_4.SelectedItem = Properties.Settings.Default.actionAppList4;
                action_app_list_5.SelectedItem = Properties.Settings.Default.actionAppList5;
                action_app_list_6.SelectedItem = Properties.Settings.Default.actionAppList6;
                action_app_list_7.SelectedItem = Properties.Settings.Default.actionAppList7;
                action_app_list_8.SelectedItem = Properties.Settings.Default.actionAppList8;
            }
            catch (Exception e) { }
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
                app_autoauth.IsChecked = Properties.Settings.Default.autoauth;
                start_delay.Text = Properties.Settings.Default.startDelay.ToString();

                action_key_phrase_1.Text = Properties.Settings.Default.actionKeyPhrase1;
                action_key_phrase_2.Text = Properties.Settings.Default.actionKeyPhrase2;
                action_key_phrase_3.Text = Properties.Settings.Default.actionKeyPhrase3;
                action_key_phrase_4.Text = Properties.Settings.Default.actionKeyPhrase4;
                action_key_phrase_5.Text = Properties.Settings.Default.actionKeyPhrase5;
                action_key_phrase_6.Text = Properties.Settings.Default.actionKeyPhrase6;
                action_key_phrase_7.Text = Properties.Settings.Default.actionKeyPhrase7;
                action_key_phrase_8.Text = Properties.Settings.Default.actionKeyPhrase8;

                action_key_press_1.Text = Properties.Settings.Default.actionKeyPress1;
                action_key_press_2.Text = Properties.Settings.Default.actionKeyPress2;
                action_key_press_3.Text = Properties.Settings.Default.actionKeyPress3;
                action_key_press_4.Text = Properties.Settings.Default.actionKeyPress4;
                action_key_press_5.Text = Properties.Settings.Default.actionKeyPress5;
                action_key_press_6.Text = Properties.Settings.Default.actionKeyPress6;
                action_key_press_7.Text = Properties.Settings.Default.actionKeyPress7;
                action_key_press_8.Text = Properties.Settings.Default.actionKeyPress8;

                action_key_ext_app_1.Text = Properties.Settings.Default.actionKeyExtApp1;
                action_key_ext_app_2.Text = Properties.Settings.Default.actionKeyExtApp2;
                action_key_ext_app_3.Text = Properties.Settings.Default.actionKeyExtApp3;
                action_key_ext_app_4.Text = Properties.Settings.Default.actionKeyExtApp4;
                action_key_ext_app_5.Text = Properties.Settings.Default.actionKeyExtApp5;
                action_key_ext_app_6.Text = Properties.Settings.Default.actionKeyExtApp6;
                action_key_ext_app_7.Text = Properties.Settings.Default.actionKeyExtApp7;
                action_key_ext_app_8.Text = Properties.Settings.Default.actionKeyExtApp8;

                action_key_exception_1.Text = Properties.Settings.Default.actionKeyException1;
                action_key_exception_2.Text = Properties.Settings.Default.actionKeyException2;
                action_key_exception_3.Text = Properties.Settings.Default.actionKeyException3;
                action_key_exception_4.Text = Properties.Settings.Default.actionKeyException4;
                action_key_exception_5.Text = Properties.Settings.Default.actionKeyException5;

                var scheduler = Properties.Settings.Default.scheduler;
                if (scheduler.Count > 0) scheduleItems = scheduler;

                try
                {
                    action_app_list_1.SelectedItem = Properties.Settings.Default.actionAppList1;
                    action_app_list_2.SelectedItem = Properties.Settings.Default.actionAppList2;
                    action_app_list_3.SelectedItem = Properties.Settings.Default.actionAppList3;
                    action_app_list_4.SelectedItem = Properties.Settings.Default.actionAppList4;
                    action_app_list_5.SelectedItem = Properties.Settings.Default.actionAppList5;
                    action_app_list_6.SelectedItem = Properties.Settings.Default.actionAppList6;
                    action_app_list_7.SelectedItem = Properties.Settings.Default.actionAppList7;
                    action_app_list_8.SelectedItem = Properties.Settings.Default.actionAppList8;
                }
                catch(Exception e) { }

                keyWinCode1 = Properties.Settings.Default.keyWinCode1;
                keyWinCode2 = Properties.Settings.Default.keyWinCode2;
                keyWinCode3 = Properties.Settings.Default.keyWinCode3;
                keyWinCode4 = Properties.Settings.Default.keyWinCode4;
                keyWinCode5 = Properties.Settings.Default.keyWinCode5;
                keyWinCode6 = Properties.Settings.Default.keyWinCode6;
                keyWinCode7 = Properties.Settings.Default.keyWinCode7;
                keyWinCode8 = Properties.Settings.Default.keyWinCode8;

                string messageFolderPath = Properties.Settings.Default.messageFolder;
                messageFolder.Text = messageFolderPath == "" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\alarm" : messageFolderPath;
                webhook_url_1.Text = Properties.Settings.Default.webhookUrl1;
            }
            catch(Exception error)
            {
                MessageBox.Show(error.ToString());
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
                for (int i = 0; i < scheduleItems.Count; i++)
                {
                    scheduleItems[i].stop();
                }

                Properties.Settings.Default.scheduler = scheduleItems;
                Properties.Settings.Default.Save();

                Application.Current.Shutdown();
            }
        }

        private void settings_open_Click(object sender, RoutedEventArgs e)
        {
            if (settingsOpen)
            {
                settings_block.Visibility = Visibility.Collapsed;
                settings_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowDown.png"));
                unactiveAnimationButton(settings_edit);
            }
            else
            {
                settings_block.Visibility = Visibility.Visible;
                settings_open_img.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/arrowUp.png"));
                activeAnimationButton(settings_edit, "editButtonAnimation");
            }

            settingsOpen = !settingsOpen;
        }

        private void lang_rus_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "RUS";
            Properties.Settings.Default.Save();

            AuthWindow.SetLanguageDictionary();
        }

        private void lang_ukr_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "UKR";
            Properties.Settings.Default.Save();

            AuthWindow.SetLanguageDictionary();
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

        private void SetAutorunProgram(bool autorun)
        {
            try
            {
                const string name = "EasyCaster Alarm";

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
            unactiveAnimationButton(settings_edit);
            enableAll();
        }
        private void enableAll()
        {
            tg_channel_main_name.IsEnabled = true;
            tg_channel_main_link.IsEnabled = true;
            tg_channel_test_name.IsEnabled = true;
            tg_channel_test_link.IsEnabled = true;

            app_autostart.IsEnabled = true;
            app_autoauth.IsEnabled = true;
            start_delay.IsEnabled = true;

            action_key_phrase_1.IsEnabled = true;
            action_key_phrase_2.IsEnabled = true;
            action_key_phrase_3.IsEnabled = true;
            action_key_phrase_4.IsEnabled = true;
            action_key_phrase_5.IsEnabled = true;
            action_key_phrase_6.IsEnabled = true;
            action_key_phrase_7.IsEnabled = true;
            action_key_phrase_8.IsEnabled = true;

            action_app_list_1.IsEnabled = true;
            action_app_list_2.IsEnabled = true;
            action_app_list_3.IsEnabled = true;
            action_app_list_4.IsEnabled = true;
            action_app_list_5.IsEnabled = true;
            action_app_list_6.IsEnabled = true;
            action_app_list_7.IsEnabled = true;
            action_app_list_8.IsEnabled = true;

            scheduler_wrapper.IsEnabled = true;

            action_key_press_1.IsEnabled = true;
            action_key_press_2.IsEnabled = true;
            action_key_press_3.IsEnabled = true;
            action_key_press_4.IsEnabled = true;
            action_key_press_5.IsEnabled = true;
            action_key_press_6.IsEnabled = true;
            action_key_press_7.IsEnabled = true;
            action_key_press_8.IsEnabled = true;

            action_key_ext_app_1.IsEnabled = true;
            action_key_ext_app_2.IsEnabled = true;
            action_key_ext_app_3.IsEnabled = true;
            action_key_ext_app_4.IsEnabled = true;
            action_key_ext_app_5.IsEnabled = true;
            action_key_ext_app_6.IsEnabled = true;
            action_key_ext_app_7.IsEnabled = true;
            action_key_ext_app_8.IsEnabled = true;

            action_key_exception_1.IsEnabled = true;
            action_key_exception_2.IsEnabled = true;
            action_key_exception_3.IsEnabled = true;
            action_key_exception_4.IsEnabled = true;
            action_key_exception_5.IsEnabled = true;

            webhook_url_1.IsEnabled = true;
            messageFolder.IsEnabled = true;
        }
        private void disableAll()
        {
            tg_channel_main_name.IsEnabled = false;
            tg_channel_main_link.IsEnabled = false;
            tg_channel_test_name.IsEnabled = false;
            tg_channel_test_link.IsEnabled = false;

            app_autostart.IsEnabled = false;
            app_autoauth.IsEnabled = false;
            start_delay.IsEnabled = false;

            action_key_phrase_1.IsEnabled = false;
            action_key_phrase_2.IsEnabled = false;
            action_key_phrase_3.IsEnabled = false;
            action_key_phrase_4.IsEnabled = false;
            action_key_phrase_5.IsEnabled = false;
            action_key_phrase_6.IsEnabled = false;
            action_key_phrase_7.IsEnabled = false;
            action_key_phrase_8.IsEnabled = false;

            action_app_list_1.IsEnabled = false;
            action_app_list_2.IsEnabled = false;
            action_app_list_3.IsEnabled = false;
            action_app_list_4.IsEnabled = false;
            action_app_list_5.IsEnabled = false;
            action_app_list_6.IsEnabled = false;
            action_app_list_7.IsEnabled = false;
            action_app_list_8.IsEnabled = false;

            scheduler_wrapper.IsEnabled = false;

            action_key_press_1.IsEnabled = false;
            action_key_press_2.IsEnabled = false;
            action_key_press_3.IsEnabled = false;
            action_key_press_4.IsEnabled = false;
            action_key_press_5.IsEnabled = false;
            action_key_press_6.IsEnabled = false;
            action_key_press_7.IsEnabled = false;
            action_key_press_8.IsEnabled = false;

            action_key_ext_app_1.IsEnabled = false;
            action_key_ext_app_2.IsEnabled = false;
            action_key_ext_app_3.IsEnabled = false;
            action_key_ext_app_4.IsEnabled = false;
            action_key_ext_app_5.IsEnabled = false;
            action_key_ext_app_6.IsEnabled = false;
            action_key_ext_app_7.IsEnabled = false;
            action_key_ext_app_8.IsEnabled = false;

            action_key_exception_1.IsEnabled = false;
            action_key_exception_2.IsEnabled = false;
            action_key_exception_3.IsEnabled = false;
            action_key_exception_4.IsEnabled = false;
            action_key_exception_5.IsEnabled = false;

            webhook_url_1.IsEnabled = false;
            messageFolder.IsEnabled = false;
        }
        private void settings_save_Click(object sender, RoutedEventArgs e)
        {
            disableAll();
            SaveSettings();
            unactiveAnimationButton(settings_save);
            activeAnimationButton(settings_edit, "editButtonAnimation");
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.tgMainChannelName = tg_channel_main_name.Text;
            Properties.Settings.Default.tgMainChannelLink = tg_channel_main_link.Text;
            Properties.Settings.Default.tgTestChannelName = tg_channel_test_name.Text;
            Properties.Settings.Default.tgTestChannelLink = tg_channel_test_link.Text;

            Properties.Settings.Default.autostart = (bool)app_autostart.IsChecked;
            Properties.Settings.Default.autoauth = (bool)app_autoauth.IsChecked;
            Properties.Settings.Default.startDelay = Convert.ToInt64(start_delay.Text);

            Properties.Settings.Default.actionKeyPhrase1 = action_key_phrase_1.Text;
            Properties.Settings.Default.actionKeyPhrase2 = action_key_phrase_2.Text;
            Properties.Settings.Default.actionKeyPhrase3 = action_key_phrase_3.Text;
            Properties.Settings.Default.actionKeyPhrase4 = action_key_phrase_4.Text;
            Properties.Settings.Default.actionKeyPhrase5 = action_key_phrase_5.Text;
            Properties.Settings.Default.actionKeyPhrase6 = action_key_phrase_6.Text;
            Properties.Settings.Default.actionKeyPhrase7 = action_key_phrase_7.Text;
            Properties.Settings.Default.actionKeyPhrase8 = action_key_phrase_8.Text;

            Properties.Settings.Default.actionAppList1 = action_app_list_1.Text;
            Properties.Settings.Default.actionAppList2 = action_app_list_2.Text;
            Properties.Settings.Default.actionAppList3 = action_app_list_3.Text;
            Properties.Settings.Default.actionAppList4 = action_app_list_4.Text;
            Properties.Settings.Default.actionAppList5 = action_app_list_5.Text;
            Properties.Settings.Default.actionAppList6 = action_app_list_6.Text;
            Properties.Settings.Default.actionAppList7 = action_app_list_7.Text;
            Properties.Settings.Default.actionAppList8 = action_app_list_8.Text;

            Properties.Settings.Default.actionKeyPress1 = action_key_press_1.Text;
            Properties.Settings.Default.actionKeyPress2 = action_key_press_2.Text;
            Properties.Settings.Default.actionKeyPress3 = action_key_press_3.Text;
            Properties.Settings.Default.actionKeyPress4 = action_key_press_4.Text;
            Properties.Settings.Default.actionKeyPress5 = action_key_press_5.Text;
            Properties.Settings.Default.actionKeyPress6 = action_key_press_6.Text;
            Properties.Settings.Default.actionKeyPress7 = action_key_press_7.Text;
            Properties.Settings.Default.actionKeyPress8 = action_key_press_8.Text;

            Properties.Settings.Default.actionKeyExtApp1 = action_key_ext_app_1.Text;
            Properties.Settings.Default.actionKeyExtApp2 = action_key_ext_app_2.Text;
            Properties.Settings.Default.actionKeyExtApp3 = action_key_ext_app_3.Text;
            Properties.Settings.Default.actionKeyExtApp4 = action_key_ext_app_4.Text;
            Properties.Settings.Default.actionKeyExtApp5 = action_key_ext_app_5.Text;
            Properties.Settings.Default.actionKeyExtApp6 = action_key_ext_app_6.Text;
            Properties.Settings.Default.actionKeyExtApp7 = action_key_ext_app_7.Text;
            Properties.Settings.Default.actionKeyExtApp8 = action_key_ext_app_8.Text;

            Properties.Settings.Default.actionKeyException1 = action_key_exception_1.Text;
            Properties.Settings.Default.actionKeyException2 = action_key_exception_2.Text;
            Properties.Settings.Default.actionKeyException3 = action_key_exception_3.Text;
            Properties.Settings.Default.actionKeyException4 = action_key_exception_4.Text;
            Properties.Settings.Default.actionKeyException5 = action_key_exception_5.Text;

            Properties.Settings.Default.keyWinCode1 = keyWinCode1;
            Properties.Settings.Default.keyWinCode2 = keyWinCode2;
            Properties.Settings.Default.keyWinCode3 = keyWinCode3;
            Properties.Settings.Default.keyWinCode4 = keyWinCode4;
            Properties.Settings.Default.keyWinCode5 = keyWinCode5;
            Properties.Settings.Default.keyWinCode6 = keyWinCode6;
            Properties.Settings.Default.keyWinCode7 = keyWinCode7;
            Properties.Settings.Default.keyWinCode8 = keyWinCode8;

            Properties.Settings.Default.webhookUrl1 = webhook_url_1.Text;
            Properties.Settings.Default.messageFolder = messageFolder.Text;

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
            startExtApp(1);
        }

        private void action_key_test_2_Click(object sender, RoutedEventArgs e)
        {
            pressKey(2);
            startExtApp(2);
        }

        private void action_key_test_3_Click(object sender, RoutedEventArgs e)
        {
            pressKey(3);
            startExtApp(3);
        }

        private void action_key_test_4_Click(object sender, RoutedEventArgs e)
        {
            pressKey(4);
            startExtApp(4);
        }

        private string getCurrectKeyName(string keyName)
        {
            if (keyName.Length == 2 && keyName[0] == 'D')
            {
                keyName = keyName[1].ToString();
            }else if (keyName == "SPACE")
            {
                keyName = "";
            }else if (keyName == "BACK")
            {
                keyName = "BACKSPACE";
            }
            else if (keyName == "RETURN")
            {
                keyName = "ENTER";
            }
            else if (keyName == "PAGEUP")
            {
                keyName = "PGUP";
            }
            else if (keyName == "NEXT")
            {
                keyName = "PGDN";
            }
            else if (keyName == "SNAPSHOT")
            {
                keyName = "PRTSC";
            }

            return keyName;
        }

        private void action_key_press_1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode1 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_1.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_2_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode2 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_2.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_3_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode3 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_3.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_4_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode4 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_4.Text = getCurrectKeyName(keyName);
        }

        private void app_logout_Click(object sender, RoutedEventArgs e)
        {
            stopAll();
            AuthWindow.isAutoauthAccept = false;
            App.authWindow.Show();
            app.IsEnabled = false;
            app.Hide();
        }


        private static readonly Dictionary<long, User> _users = new();
        private static readonly Dictionary<long, ChatBase> _chats = new();
        private static string User(long id) => _users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string Chat(long id) => _chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        private static string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";

        public async Task Client_Update(IObject arg)
        {
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(_users, _chats);

            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm: DisplayMessage(unm.message); break;
                }
        }

        private void DisplayMessage(MessageBase messageBase, bool edit = false)
        {
            switch (messageBase)
            {
                case Message m: parseMessageAndFindStopPhrases(m); break;
            }
        }

        private bool isNoException(string message)
        {
            string exception1 = action_key_exception_1.Text.ToLower().Trim();
            string exception2 = action_key_exception_2.Text.ToLower().Trim();
            string exception3 = action_key_exception_3.Text.ToLower().Trim();
            string exception4 = action_key_exception_4.Text.ToLower().Trim();
            string exception5 = action_key_exception_5.Text.ToLower().Trim();

            List<string> exceptions = new List<string> { exception1, exception2, exception3, exception4, exception5 };

            for(int i = 0; i < exceptions.Count; i++)
            {
                if (exceptions[i] != "" && message.IndexOf(exceptions[i]) != -1) return false;
            }

            return true;
        }

        private  void parseMessageAndFindStopPhrases(Message m)
        {
            string source = Peer(m.peer_id).ToLower();
            string message = m.message.ToLower();
            bool isException = !isNoException(message);

            string mainChannelLink = tg_channel_main_link.Text.ToLower().Trim();
            string testChannelLink = tg_channel_test_link.Text.ToLower().Trim();

            string keyPhrase1 = action_key_phrase_1.Text.ToLower().Trim();
            string keyPhrase2 = action_key_phrase_2.Text.ToLower().Trim();
            string keyPhrase3 = action_key_phrase_3.Text.ToLower().Trim();
            string keyPhrase4 = action_key_phrase_4.Text.ToLower().Trim();
            string keyPhrase5 = action_key_phrase_5.Text.ToLower().Trim();
            string keyPhrase6 = action_key_phrase_6.Text.ToLower().Trim();
            string keyPhrase7 = action_key_phrase_7.Text.ToLower().Trim();
            string keyPhrase8 = action_key_phrase_8.Text.ToLower().Trim();

            string[] mainChannelLinkArray = mainChannelLink.Split('/');
            string[] testChannelLinkArray = testChannelLink.Split('/');

            mainChannelLink = mainChannelLinkArray[mainChannelLinkArray.Length - 1];
            testChannelLink = testChannelLinkArray[testChannelLinkArray.Length - 1];

            if (
                ((mainChannelLink != "" && source.IndexOf(mainChannelLink) != -1) ||
                (testChannelLink != "" && source.IndexOf(testChannelLink) != -1)) &&
                !isException &&
                message != ""
                )
            {
                if (keyPhrase1 != "" && message.IndexOf(keyPhrase1) != -1)
                {
                    doAlarm(keyPhrase1, 1, message);
                }
                else if (keyPhrase2 != "" &&  message.IndexOf(keyPhrase2) != -1)
                {
                    doAlarm(keyPhrase2, 2, message);
                }
                else if (keyPhrase3 != "" &&  message.IndexOf(keyPhrase3) != -1)
                {
                    doAlarm(keyPhrase3, 3, message);
                }
                else if (keyPhrase4 != "" &&  message.IndexOf(keyPhrase4) != -1)
                {
                    doAlarm(keyPhrase4, 4, message);
                }
                else if (keyPhrase5 != "" && message.IndexOf(keyPhrase5) != -1)
                {
                    doAlarm(keyPhrase5, 5, message);
                }
                else if (keyPhrase6 != "" && message.IndexOf(keyPhrase6) != -1)
                {
                    doAlarm(keyPhrase6, 6, message);
                }
                else if (keyPhrase7 != "" && message.IndexOf(keyPhrase7) != -1)
                {
                    doAlarm(keyPhrase7, 7, message);
                }
                else if (keyPhrase8 != "" && message.IndexOf(keyPhrase8) != -1)
                {
                    doAlarm(keyPhrase8, 8, message);
                }
            }
        }

        public void activeAnimationButton(Button button, string animationName)
        {
            BrushConverter bc = new BrushConverter();
            button.BorderBrush = (Brush)bc.ConvertFrom("#FFD6B656");
            button.Background = (Brush)bc.ConvertFrom("#FFFFDC03");

            DoubleAnimation da = new DoubleAnimation();

            da.From = 1.0;
            da.To = 0.5;
            da.RepeatBehavior = RepeatBehavior.Forever;
            da.AutoReverse = true;

            sb = TryFindResource(animationName) as Storyboard;
            sb.Children.Add(da);
            Storyboard.SetTargetProperty(da, new PropertyPath("(Button.Opacity)"));
            Storyboard.SetTarget(da, button);

            sb.Begin();
        }

        public void unactiveAnimationButton(Button button)
        {
            try
            {
                BrushConverter bc = new BrushConverter();
                button.BorderBrush = (Brush)bc.ConvertFrom("#FF707070");
                button.Background = (Brush)bc.ConvertFrom("#FFF0F0F0");

                if (sb != null) sb.Stop();
            }
            catch (Exception e)
            {

            }
        }

        private void tg_channel_main_name_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tg_channel_main_name.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void tg_channel_main_link_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tg_channel_main_link.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void tg_channel_test_name_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tg_channel_test_name.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void tg_channel_test_link_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tg_channel_test_link.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void app_autostart_Click(object sender, RoutedEventArgs e)
        {
            bool autostart = (bool)app_autostart.IsChecked;
            SetAutorunProgram(autostart);

            activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_1.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_1.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_1.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_2.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_2.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_2.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_3.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_3.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_3.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_4.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_4.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_4.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_exception_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_exception_1.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_exception_2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_exception_2.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_exception_3_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_exception_3.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_exception_4_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_exception_4.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void app_autoauth_Click(object sender, RoutedEventArgs e)
        {
            activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_exception_5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_exception_5.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_test_5_Click(object sender, RoutedEventArgs e)
        {
            pressKey(5);
            startExtApp(5);
        }

        private void action_key_test_6_Click(object sender, RoutedEventArgs e)
        {
            pressKey(6);
            startExtApp(6);
        }

        private void action_key_test_7_Click(object sender, RoutedEventArgs e)
        {
            pressKey(7);
            startExtApp(7);
        }

        private void action_key_test_8_Click(object sender, RoutedEventArgs e)
        {
            pressKey(8);
            startExtApp(8);
        }

        private void action_app_list_5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_5.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_6_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_6.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_7_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_7.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_app_list_8_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_app_list_8.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_5_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode5 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_5.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_6_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode6 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_6.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_7_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode7 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_7.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_8_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode8 = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            action_key_press_8.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_5.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_6_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_6.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_7_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_7.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_press_8_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_press_8.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_5_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_5.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_6_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_6.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_7_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_7.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_phrase_8_LostFocus(object sender, RoutedEventArgs e)
        {
            if (action_key_phrase_8.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void messageFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (messageFolder.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void messageFolderOpen_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            messageFolder.Text = folderBrowserDialog1.SelectedPath;
            activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void messageFolder_GotFocus(object sender, RoutedEventArgs e)
        {
            string path = messageFolder.Text;
            if (path == Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\alarm") messageFolder.Text = "";
        }

        private void messageFolder_LostFocus_1(object sender, RoutedEventArgs e)
        {
            string path = messageFolder.Text;
            if (path == "") messageFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\alarm";
        }

        private void webhook_test_1_Click(object sender, RoutedEventArgs e)
        {
            if (webhook_url_1.Text != "") sendWebhook(webhook_url_1.Text);
        }

        private void action_key_ext_app_1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectExtApp(action_key_ext_app_1);
        }

        private void selectExtApp(TextBox el)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                el.Text = openFileDialog.FileName;
            activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void action_key_ext_app_1_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_1);
        }

        private void action_key_ext_app_2_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_2);
        }

        private void action_key_ext_app_3_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_3);
        }

        private void action_key_ext_app_4_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_4);
        }

        private void action_key_ext_app_5_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_5);
        }

        private void action_key_ext_app_6_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_6);
        }

        private void action_key_ext_app_7_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_7);
        }

        private void action_key_ext_app_8_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(action_key_ext_app_8);
        }

        private void webhook_url_1_LostFocus(object sender, RoutedEventArgs e)
        {
            if(webhook_url_1.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void scheduler_action_app_list_LostFocus(object sender, RoutedEventArgs e)
        {
            if (scheduler_action_app_list.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void scheduler_action_key_press_KeyUp(object sender, KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            schedulerKeyWinCode = (int)KeyInterop.VirtualKeyFromKey(e.Key);
            scheduler_action_key_press.Text = getCurrectKeyName(keyName);
        }

        private void scheduler_action_key_press_LostFocus(object sender, RoutedEventArgs e)
        {
            if (scheduler_action_key_press.Text != "") activeAnimationButton(settings_save, "saveButtonAnimation");
        }

        private void scheduler_action_key_ext_app_GotFocus(object sender, RoutedEventArgs e)
        {
            selectExtApp(scheduler_action_key_ext_app);
        }

        private void scheduler_test_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string applicationName = processAppNames[scheduler_action_app_list.SelectedIndex];
                string externalApp = scheduler_action_key_ext_app.Text;
                int keyWinCode = schedulerKeyWinCode;

                Process p = Process.GetProcessesByName(applicationName).FirstOrDefault();
                Process currentProcess = Process.GetCurrentProcess();

                if (currentProcess != null && p != null)
                {
                    IntPtr h = p.MainWindowHandle;

                    DispatcherTimer appTimer = new DispatcherTimer();
                    appTimer.Interval = TimeSpan.FromMilliseconds(500);
                    appTimer.Tick += app_Timer;
                    appTimer.Start();

                    void app_Timer(object sender, EventArgs e)
                    {
                        ShowWindow(h, 9);
                        SetWindowPos(h, HWND_TOP, 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);
                        SwitchToThisWindow(h, true);

                        InputSimulator s = new InputSimulator();
                        s.Keyboard.KeyPress((VirtualKeyCode)keyWinCode);

                        appTimer.Stop();
                    }
                }

                if (externalApp != "") Process.Start(externalApp);
            }
            catch (Exception error) { }
        }

        private void scheduler_add_new_Click(object sender, RoutedEventArgs e)
        {
            if (scheduler_select_start.Text == "")
            {
                MessageBox.Show(FindResource("scheduler_error_1").ToString(), FindResource("scheduler_error_1").ToString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (scheduler_select_stop.Text == "")
            {
                MessageBox.Show(FindResource("scheduler_error_2").ToString(), FindResource("scheduler_error_2").ToString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (scheduler_action_frequency.Text == "")
            {
                MessageBox.Show(FindResource("scheduler_error_3").ToString(), FindResource("scheduler_error_3").ToString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (processAppNames.Count < scheduler_action_app_list.SelectedIndex || scheduler_action_app_list.SelectedIndex < 0)
            {
                MessageBox.Show(FindResource("scheduler_error_4").ToString(), FindResource("scheduler_error_3").ToString(), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int select_start = Convert.ToInt32(scheduler_select_start.Text);
            int select_end = Convert.ToInt32(scheduler_select_stop.Text);
            string app_list_item = processAppNames[scheduler_action_app_list.SelectedIndex];
            string ext_app = scheduler_action_key_ext_app.Text;
            int frequency = Convert.ToInt32(scheduler_action_frequency.Text);

            var scheduleItem = new ScheduleItem(select_start, select_end, app_list_item, scheduler_action_key_press.Text, schedulerKeyWinCode, ext_app, frequency);
            scheduleItems.Insert(0, scheduleItem);


            Properties.Settings.Default.scheduler = scheduleItems;
            Properties.Settings.Default.Save();

            scheduler_select_start.Text = "";
            scheduler_select_stop.Text = "";
            scheduler_action_app_list.Text = "";
            scheduler_action_key_press.Text = "";
            scheduler_action_key_ext_app.Text = "";
            scheduler_action_frequency.Text = "";
            schedulerKeyWinCode = 0;
        }

        private void scheduler_clear_all_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < scheduleItems.Count; i++)
            {
                scheduleItems[i].stop();
            }

            Properties.Settings.Default.scheduler = new ObservableCollection<ScheduleItem> { };
            ((ObservableCollection<ScheduleItem>)scheduler_list.ItemsSource).Clear();
            Properties.Settings.Default.Save();
        }

        private void scheduler_item_remove_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var buttonImage = sender as Image;

            if (buttonImage != null)
            {
                var schedule_item = buttonImage.DataContext as ScheduleItem;
                schedule_item.stop();
                ((ObservableCollection<ScheduleItem>)scheduler_list.ItemsSource).Remove(schedule_item);
                Properties.Settings.Default.scheduler = scheduleItems;
                Properties.Settings.Default.Save();
            }
            else
            {
                return;
            }
        }

        private void updateSchedulers(byte activeIndex)
        {
            for(int i = 0; i < scheduleItems.Count; i++)
            {
                ScheduleItem scheduleItem = scheduleItems[i];
                if (scheduleItem.select_start == activeIndex) scheduleItem.start();
                if (scheduleItem.select_end == activeIndex) scheduleItem.stop();
            }
        }
    }
}
