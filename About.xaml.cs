using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        int easterEggCounter = 0;
        public About()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            v_lb.Content = v_lb.Content + " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
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

        private void v_lb_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(easterEggCounter >= 5)
            {
                MessageBox.Show("Power by legdev (Zutar, 2022)");
            }
            else
            {
                easterEggCounter++;
            }
        }
    }
}
