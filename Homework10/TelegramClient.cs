using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Homework10
{
    class TelegramClient
    {
        #region Поля

        //Окно
        private MainWindow Window;

        //Телеграм клиент
        private TelegramBotClient Client;

        //Коллекция пользователей
        public ObservableCollection<User> ClientUsersList { get; set; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор телеграм клиента
        /// </summary>
        /// <param name="window">Окно</param>
        /// <param name="token">Идентификационный номер чат-бота</param>
        public TelegramClient(MainWindow window, string token)
        {
            Window = window;
            Client = new TelegramBotClient(token);
            ClientUsersList = new ObservableCollection<User>();

            //Старт прослушивания входящих сообщений
            Client.StartReceiving();
            //Реакция на входящее сообщение
            Client.OnMessage += ReactionOnMessage;
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод, реализующий реакцию чат-бота на входящее сообщение
        /// </summary>
        private async void ReactionOnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var msg = e.Message;
            await Client.SendTextMessageAsync(msg.Chat.Id, "Выберите функцию или загрузите файл", replyMarkup: GetButtons());
            await Task.Delay(10);

            TypeMessages(msg);

            Window.Dispatcher.Invoke(() =>
            {
                bool flag = true;
                foreach (var user in ClientUsersList)
                {
                    if (user.Id == e.Message.Chat.Id)
                    {
                        flag = false;
                        user.UserMessagesList.Add(new Message(DateTime.Now, e.Message.Text));
                        break;
                    }
                }
                if (flag)
                {
                    ClientUsersList.Add(new User(e.Message.Chat.FirstName, e.Message.Chat.LastName, e.Message.Chat.Id));
                    ClientUsersList.Last<User>().UserMessagesList.Add(new Message(DateTime.Now, e.Message.Text));
                }
            });
        }

        //Метод, возвращающий функциональные клавиши чат-бота
        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{new KeyboardButton { Text ="Показать список загруженных файлов"}, new KeyboardButton {Text="Показать список команд" } }
                }
            };
        }

        /// <summary>
        /// Метод, реализующий реакцию чат-бота на сообщение, в зависимости от его типа
        /// </summary>
        /// <param name="message">Объект телеграм-сообщение</param>
        private async void TypeMessages(Telegram.Bot.Types.Message message)
        {
            switch (message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:

                    IfMessagesText(message.Chat.Id, message.Text);
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Photo:

                    await Client.SendTextMessageAsync(message.Chat.Id, "Найс фото!");
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Video:

                    await Client.SendTextMessageAsync(message.Chat.Id, "Найс видео!");
                    break;

                case Telegram.Bot.Types.Enums.MessageType.Document:

                    UploadFile(message.Chat.Id.ToString(), message.Document.FileId, message.Document.FileName);
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Метод, реализующий выбор действия бота в случае, если присланное сообщение было текстовым
        /// </summary>
        /// <param name="id">id пользователя в телеграме</param>
        /// <param name="text">текст входящего сообщения</param>
        private async void IfMessagesText(long id, string text)
        {
            if (text == "/start")
            {
                string Text = "Добро пожаловать в бота - файловое хранилище";
                await Client.SendTextMessageAsync(id, Text);
            }

            if (text == "Показать список загруженных файлов")
                FileInDirectory(id.ToString());

            if (text == "Показать список команд")
            {
                string Text = "/start - для начала общения с ботом\n" +
                    "/Download i - для скачивания файла, где i - его номер из списка загруженных файлов\n" +
                    "/DownloadAll - скачать все загруженные файлы";
                await Client.SendTextMessageAsync(id, Text);
            }
            if (text.StartsWith("/Download "))
            {
                if (int.TryParse(text.Substring(10), out int i))
                    Download(id.ToString(), i);

                else await Client.SendTextMessageAsync(id, "Введите корректную команду");
            }

            if (text == "/DownloadAll")
                DownloadAll(id.ToString());
        }

        /// <summary>
        /// Метод, реализующий скачивание выбранного файла
        /// </summary>
        /// <param name="chatid">Id чата пользователя</param>
        /// <param name="i">Номер файла из списка загруженных файлов</param>
        private async void Download(string chatid, int i)
        {
            var directory = Directory.CreateDirectory(chatid);
            var files = Directory.GetFiles(chatid);
            try
            {
                using (FileStream fs = new FileStream(files[i - 1], FileMode.Open))
                {
                    InputOnlineFile iof = new InputOnlineFile(fs);
                    iof.FileName = files[i - 1].Substring(10);
                    var send = await Client.SendDocumentAsync(chatid, iof);
                }
            }
            catch
            {
                await Client.SendTextMessageAsync(chatid, "Введите номер из диапазона Вашего списка файлов");
            }
        }

        /// <summary>
        /// Метод, скачивающий все файлы из списка загруженных файлов
        /// </summary>
        /// <param name="chatid">Id чата пользователя</param>
        private async void DownloadAll(string chatid)
        {
            var directory = Directory.CreateDirectory(chatid);
            var files = Directory.GetFiles(chatid.ToString());
            foreach (var f in files)
            {
                using (FileStream fs = new FileStream(f, FileMode.Open))
                {
                    InputOnlineFile iof = new InputOnlineFile(fs);
                    iof.FileName = f.Substring(10);
                    var send = await Client.SendDocumentAsync(chatid, iof);
                    await Task.Delay(10);
                }
            }
        }

        /// <summary>
        /// Метод, загружающий на сервер файла, отправленный пользователем в чат
        /// </summary>
        /// <param name="chatid">id чата пользователя</param>
        /// <param name="fileid">id файла, отправленного в чат</param>
        /// <param name="FileName">Имя файла</param>
        private async void UploadFile(string chatid, string fileid, string FileName)
        {
            var file = await Client.GetFileAsync(fileid);
            var directory = Directory.CreateDirectory(chatid);
            using (FileStream fs = new FileStream(chatid + @"\" + FileName, FileMode.Create))
            {
                await Client.DownloadFileAsync(file.FilePath, fs);
            }
        }

        /// <summary>
        /// Метод, выводящий в чат список, загруженных пользователем, файлов
        /// </summary>
        /// <param name="chatid">Id чата пользвателя</param>
        private async void FileInDirectory(string chatid)
        {
            var directory = Directory.CreateDirectory(chatid);
            var files = Directory.GetFiles(chatid);
            int count = 1;
            foreach (var f in files)
            {
                await Client.SendTextMessageAsync(chatid, $"{count}){f.Substring(11)}");
                count += 1;
                await Task.Delay(10);
            }
        }

        /// <summary>
        /// Метод, отправки чат-ботом сообщения пользователю
        /// </summary>
        /// <param name="Text">Текст сообщения</param>
        /// <param name="Id">Id пользователя в телеграме</param>
        public async void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            await Client.SendTextMessageAsync(id, Text);

            //Поиск id пользователя в коллекции пользователей и добавление сообщения в коллекцию сообщений
            foreach (var user in ClientUsersList)
            {
                if (user.Id == id)
                {
                    user.UserMessagesList.Add(new Message(DateTime.Now, Text));
                    break;
                }
            }
        }

        #endregion
    }
}