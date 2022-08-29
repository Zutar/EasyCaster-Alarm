using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace EasyCaster_Alarm.classes
{
    [Serializable()]
    public class ScheduleItem
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

        public int select_start { get; set; }
        public int select_end { get; set; }
        public string app_list_item { get; set; }
        public string key_press { get; set; }
        public int key_press_code { get; set; }
        public string ext_app { get; set; }
        public int frequency { get; set; }
        public string bgColor { get; set; }

        [NonSerialized]
        public DispatcherTimer timer = null;

        public ScheduleItem(int select_start, int select_end, string app_list_item, string key_press, int key_press_code, string ext_app, int frequency)
        {
            this.select_start = select_start;
            this.select_end = select_end;
            this.app_list_item = app_list_item;
            this.key_press = key_press;
            this.key_press_code = key_press_code;
            this.ext_app = ext_app;
            this.frequency = frequency;
        } 

        public void start()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(frequency > 0 ? frequency : 1);
            timer.Tick += scheduler_Timer;
            timer.Start();

            void scheduler_Timer(object sender, EventArgs e)
            {
                pressKey();
                startExtApp();
            }
        }

        public void stop()
        {
            if (timer != null) timer.Stop();
        }

        private void startExtApp()
        {
            try
            {
                Process.Start(ext_app);
            }
            catch (Exception error)
            {

            }
        }

        private void pressKey()
        {
            try
            {
                string applicationName = app_list_item;
                int keyWinCode = key_press_code;


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
            }
            catch (Exception error) { }
        }
    }
}
