using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow()
        {
            InitializeComponent();
            error_message.Text = App.errorMessage;
        }

        private void send_and_close_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)send_data.IsChecked)
            {
                App.sendEmailMessage("Easycaster-Alarm " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ": unhandled exception", App.errorMessage);
            }

            this.Close();
        }
    }
}
