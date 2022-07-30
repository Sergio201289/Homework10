using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework10
{
    class Message
    {
        #region Поля

        //Тескт
        [JsonProperty("Text")]
        public string Text { get; set; }

        //Время отправки
        [JsonProperty("DateTime")]
        public DateTime DateTime { get; set; }

        #endregion

        #region Конструктор

        //Конструктор сообщения
        public Message(DateTime dateTime, string text)
        {
            DateTime = dateTime;
            Text = text;
        }

        #endregion
    }
}