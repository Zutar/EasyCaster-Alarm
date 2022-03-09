using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TL;
using WTelegram;

namespace EasyCaster_Alarm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Client client;
        public static AuthWindow authWindow = new AuthWindow();
        static MainWindow mainWindow = new MainWindow();

        public App()
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            //authWindow.ShowDialog();
            //mainWindow.IsEnabled = false;
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

        public static async void createTGClient(Func<string, string> configProvider = null)
        {
            client = new WTelegram.Client(configProvider);

            try
            {
                var my = await client.LoginUserIfNeeded();

                MessageBox.Show($"We are logged-in as {my.username ?? my.first_name + " " + my.last_name} (id {my.id})");

                authWindow.Hide();
                mainWindow.IsEnabled = true;

                client.Update += Client_Update;

                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += timer_Tick;
                timer.Start();

                async void timer_Tick(object sender, EventArgs e)
                {
                    var dialogs = await client.Messages_GetAllDialogs();
                    dialogs.CollectUsersChats(_users, _chats);
                }
            }catch(Exception error)
            {
                authWindow.Show();
                mainWindow.IsEnabled = false;
            }
        }

        private static readonly Dictionary<long, User> _users = new();
        private static readonly Dictionary<long, ChatBase> _chats = new();
        private static string User(long id) => _users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
        private static string Chat(long id) => _chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
        private static string Peer(Peer peer) => peer is null ? null : peer is PeerUser user ? User(user.user_id)
            : peer is PeerChat or PeerChannel ? Chat(peer.ID) : $"Peer {peer.ID}";

        private static void Client_Update(IObject arg)
        {
            MessageBox.Show("t1");
            if (arg is not UpdatesBase updates) return;
            updates.CollectUsersChats(_users, _chats);
   
            foreach (var update in updates.UpdateList)
                switch (update)
                {
                    case UpdateNewMessage unm: DisplayMessage(unm.message); break;
                    default: MessageBox.Show(update.GetType().Name); break; // there are much more update types than the above cases
                }
        }

        private static void DisplayMessage(MessageBase messageBase, bool edit = false)
        {
            switch (messageBase)
            {
                case Message m: MessageBox.Show($"{Peer(m.from_id) ?? m.post_author} in {Peer(m.peer_id)}> {m.message}"); break;
                case MessageService ms: MessageBox.Show($"{Peer(ms.from_id)} in {Peer(ms.peer_id)} [{ms.action.GetType().Name[13..]}]"); break;
            }
        }
    }
}
