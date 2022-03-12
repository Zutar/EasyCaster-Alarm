using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
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
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;


        bool settingsOpen = false;
        bool logsOpen = true;
        bool isAlarm = false;

        Storyboard sb;

        ObservableCollection<LogItem> logs = new ObservableCollection<LogItem>();
        ObservableCollection<string> processAppNames = new ObservableCollection<string> { };

        DispatcherTimer updateProcessListTimer = new DispatcherTimer();
        DispatcherTimer waitTimer = new DispatcherTimer();
        DispatcherTimer timer = new DispatcherTimer();

        Key keyWinCode1;
        Key keyWinCode2;
        Key keyWinCode3;
        Key keyWinCode4;

        public MainWindow()
        {
            InitializeComponent();

            setSavedSettings();

            DispatcherTimer startTimer = new DispatcherTimer();
            startTimer.Interval = TimeSpan.FromMilliseconds(Convert.ToInt64(start_delay.Text));
            startTimer.Tick += start_Timer;
            startTimer.Start();

            void start_Timer(object sender, EventArgs e)
            {
                start();
                startTimer.Stop();
            }
        }
        
        private void start()
        {
            App.client.Update += Client_Update;

            getActiveProcessNames();

            settings_block.Visibility = Visibility.Collapsed;
            logs_block.Visibility = Visibility.Visible;

            logs_list.ItemsSource = logs;

            updateProcessListTimer.Interval = TimeSpan.FromSeconds(5);
            updateProcessListTimer.Tick += updateProcessList_Timer;
            updateProcessListTimer.Start();

            void updateProcessList_Timer(object sender, EventArgs e)
            {
                getActiveProcessNames();
            }

            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();

            async void timer_Tick(object sender, EventArgs e)
            {
                BrushConverter bc = new BrushConverter();

                bool connectionStatus = App.client.Disconnected;
                if (connectionStatus)
                {
                    status.Background = (Brush)bc.ConvertFrom("#FFFF0000");
                }
                else
                {
                    status.Background = (Brush)bc.ConvertFrom("#FF2FC300");
                    await App.client.ConnectAsync();
                }

                var dialogs = await App.client.Messages_GetAllDialogs();
                dialogs.CollectUsersChats(_users, _chats);
            }
        }

        private void stopAll()
        {
            if(updateProcessListTimer != null) updateProcessListTimer.Stop();
            if (waitTimer != null) waitTimer.Stop();
            if (timer != null) timer.Stop();

            isAlarm = false;
            App.client.Auth_LogOut();
            App.client = null;

            disableAll();
        }

        private void pressKey(byte index)
        {
            try
            {
                string applicationName = "";
                Key keyFormCode = new Key();
                int keyWinCode = 0;

                if (index == 1)
                {
                    applicationName = processAppNames[action_app_list_1.SelectedIndex];
                    keyFormCode = keyWinCode1;
                }
                else if (index == 2)
                {
                    applicationName = processAppNames[action_app_list_2.SelectedIndex];
                    keyFormCode = keyWinCode2;
                }
                else if (index == 3)
                {
                    applicationName = processAppNames[action_app_list_3.SelectedIndex];
                    keyFormCode = keyWinCode3;
                }
                else if (index == 4)
                {
                    applicationName = processAppNames[action_app_list_4.SelectedIndex];
                    keyFormCode = keyWinCode4;
                }

                keyWinCode = (int)KeyInterop.VirtualKeyFromKey(keyFormCode);

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
                        SetForegroundWindow(h);
                        SetWindowPos(h, HWND_TOPMOST, 0, 0, 0, 0, SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE);
                        SwitchToThisWindow(h, true);

  
                        InputSimulator s = new InputSimulator();
                        s.Keyboard.KeyPress((VirtualKeyCode)keyWinCode);

                        appTimer.Stop();
                    }
                }
            }catch(Exception error) { }
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

        private void doAlarm(string message, byte index)
        {
            alarm_block.Visibility = Visibility.Visible;
            isAlarm = true;

            Storyboard alarmBlinkAnimation = TryFindResource("alarmBlinkAnimation") as Storyboard;
            if (alarmBlinkAnimation != null)
            {
                alarmBlinkAnimation.Begin();
            }

            addAlarmLog(message);
            pressKey(index);

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

        private void getActiveProcessNames()
        {
            processAppNames.Clear();

            var processes = Process.GetProcesses();

            action_app_list_1.Items.Clear();
            action_app_list_2.Items.Clear();
            action_app_list_3.Items.Clear();
            action_app_list_4.Items.Clear();

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
                }
            }

            action_app_list_1.Items.Refresh();
            action_app_list_2.Items.Refresh();
            action_app_list_3.Items.Refresh();
            action_app_list_4.Items.Refresh();
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
            AuthWindow.SetLanguageDictionary();
        }

        private void lang_ukr_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.lang = "UKR";
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

        private void SetAutorunProgram(bool autorun)
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
            tg_channel_main_name.IsEnabled = false;
            tg_channel_main_link.IsEnabled = false;
            tg_channel_test_name.IsEnabled = false;
            tg_channel_test_link.IsEnabled = false;

            app_autostart.IsEnabled = false;
            app_autoauth.IsEnabled = false;

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
            keyWinCode1 = e.Key;
            action_key_press_1.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_2_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode2 = e.Key;
            action_key_press_2.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_3_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode3 = e.Key;
            action_key_press_3.Text = getCurrectKeyName(keyName);
        }

        private void action_key_press_4_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string keyName = e.Key.ToString().ToUpper();
            keyWinCode4 = e.Key;
            action_key_press_4.Text = getCurrectKeyName(keyName);
        }

        private void app_logout_Click(object sender, RoutedEventArgs e)
        {
            stopAll();

            App.authWindow.Show();
            app.IsEnabled = false;
        }


        private static readonly Dictionary<long, User> _users = new();
        private static readonly Dictionary<long, ChatBase> _chats = new();
        private static string User(long id) => _users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string Chat(long id) => _chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        private static string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";

        public void Client_Update(IObject arg)
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

            List<string> exceptions = new List<string> { exception1, exception2, exception3, exception4 };

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
                    doAlarm(keyPhrase1, 1);
                }
                else if (keyPhrase2 != "" &&  message.IndexOf(keyPhrase2) != -1)
                {
                    doAlarm(keyPhrase2, 2);
                }
                else if (keyPhrase3 != "" &&  message.IndexOf(keyPhrase3) != -1)
                {
                    doAlarm(keyPhrase3, 3);
                }
                else if (keyPhrase4 != "" &&  message.IndexOf(keyPhrase4) != -1)
                {
                    doAlarm(keyPhrase4, 4);
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
    }
}
