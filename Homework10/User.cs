using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using System.Collections.ObjectModel;

namespace Homework10
{
    class User
    {
        #region Поля

        //Имя
        public string FirstName { get; set; }

        //Фамилия
        public string LastName { get; set; }

        //Id в телеграме
        public long Id { get; set; }

        //Коллекция сообщений пользователя
        public ObservableCollection<Message> UserMessagesList { get; set; }

        #endregion

        #region Конструктор

        //Конструктор пользователя
        public User(string firstName, string lastname,long id)
        {
            FirstName = firstName;
            LastName = lastname;
            Id = id;
            UserMessagesList = new ObservableCollection<Message>();
        }

        #endregion
    }
}