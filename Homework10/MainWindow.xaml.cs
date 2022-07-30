using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telegram.Bot;

namespace Homework10
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TelegramClient client;
        public MainWindow()
        {
            InitializeComponent();
            string token = "2144777601:AAFiWU_uo47gRm62GiWugioo9rK_7TK8ebw";
            client = new TelegramClient(this, token);
            ListUsers.ItemsSource = client.ClientUsersList;
        }

        private void btnMsgSend_Click(object sender, RoutedEventArgs e)
        {
            if (!(txtMsgSend.Text=="")&&!(TargetSend.Text=="")) client.SendMessage(txtMsgSend.Text, TargetSend.Text);
        }

        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void SaveMessages_Click(object sender, RoutedEventArgs e)
        {
            //Поиск пользователя в коллекции пользователей
            foreach(var user in client.ClientUsersList)
            {
                if (user.Id == Convert.ToInt64(TargetSend.Text))
                {
                    //Сериализация коллекции сообщений пользователя
                    File.WriteAllText($"{TargetSend.Text}.json", JsonConvert.SerializeObject(user.UserMessagesList));
                    break;
                }
            }
        }

        private void ShowFileList_Click(object sender, RoutedEventArgs e)
        {
            if (TargetSend.Text != "")
            {
                var directory = Directory.CreateDirectory(TargetSend.Text);

                //Получение массива данных о файлах
                var files = Directory.GetFiles(TargetSend.Text);
                int count = 1;
                string FilesInDirectory = "";

                //Перебор массива
                foreach (var f in files)
                {
                    //Формаирование строки-списка файлов
                    FilesInDirectory += $"{count}){f.Substring(11)}\n";
                    count += 1;
                }
                MessageBox.Show(FilesInDirectory);
            }
        }
    }
}