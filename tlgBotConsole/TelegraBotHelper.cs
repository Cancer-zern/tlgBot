using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace tlgBotConsole
{
    internal class TelegraBotHelper
    {

        private const string TEXT_1 = "Установка";
        private const string TEXT_2 = "Проблема";
        private const string TEXT_3 = "Другое";
        private const string TEXT_4 = "Что-то еще";
        private const string CAT_1 = "Камеры";
        private const string CAT_2 = "Домофон";
        private const string CAT_3 = "Сервер(ы)";
        private const string CAT_4 = "1С Предприятие";
        private const string CANCEL = "Отмена";

        private string _token;
        Telegram.Bot.TelegramBotClient _client;
        private Dictionary<long, UserState> _clientStates = new Dictionary<long, UserState>();

        public TelegraBotHelper(string token)
        {
            this._token = token;
        }

        internal void GetUpdates()
        {
            _client = new Telegram.Bot.TelegramBotClient(_token);
            var me = _client.GetMeAsync().Result;
            if (me != null && !string.IsNullOrEmpty(me.Username))
            {
                int offset = 0;
                while (true)
                {
                    try
                    {
                        var updates = _client.GetUpdatesAsync(offset).Result;
                        if (updates != null && updates.Count() > 0)
                        {
                            foreach (var update in updates)
                            {
                                processUpdate(update);
                                offset = update.Id + 1;
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }

                    //Thread.Sleep(1000);
                    Task.Delay(1000);
                }
            }
        }


        EmailForm inst = new EmailForm();

        private void processUpdate(Telegram.Bot.Types.Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;

                    // TODO: need to code refactoring in the feature and build this app in Asp.Net
                    if (text == CANCEL)
                    {
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                        inst.Clear();
                        break;
                    }

                    if (inst.TicketType != null & inst.TicketCategory != null & inst.UserName == null)
                    {
                        inst.UserName = text;
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона (пример 89281234567)", replyMarkup: CancelButton());
                        break;
                    }

                    if (inst.TicketType != null & inst.TicketCategory != null & inst.UserName != null)
                    {
                        if (text.All(char.IsDigit) & text.Length == 11)
                        {
                            inst.PhoneNumber = text;
                            // create random no of ticket
                            inst.TicketNumber = RandomNo();
                            // just for cw
                            Console.WriteLine(inst.TicketNumber);
                            Console.WriteLine(inst.TicketType);
                            Console.WriteLine(inst.TicketCategory);
                            Console.WriteLine(inst.UserName);
                            Console.WriteLine(inst.PhoneNumber);
                            Console.WriteLine("------------");
                            _client.SendTextMessageAsync(update.Message.Chat.Id, $"Спасибо, {inst.UserName}! Ваша заявка №{inst.TicketNumber} принята.");
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Перенаправляю вас в Главное меню!", replyMarkup: GetButtons());
                            // run method for sendind tlgChannel
                            SendTlgChannel(inst.TicketType, inst.TicketCategory, inst.TicketNumber, inst.UserName, inst.PhoneNumber);
                            // run method for sendind Email
                            SendAsync(inst.TicketType, inst.TicketCategory, inst.TicketNumber, inst.UserName, inst.PhoneNumber);
                            inst.Clear();
                        }
                        else
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона (пример 89281234567)", replyMarkup: CancelButton());
                        break;
                    }

                    switch (text)
                    {

                        case TEXT_1:
                        case TEXT_2:
                        case TEXT_3:
                        case TEXT_4:
                            inst.TicketType = text;
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите категорию в меню", replyMarkup: GetCategory());
                            break;

                        case CAT_1:
                        case CAT_2:
                        case CAT_3:
                        case CAT_4:
                            inst.TicketCategory = text;
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя", replyMarkup: CancelButton());
                            break;

                        //case CANCEL:
                        //    _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                        //    inst.Clear();
                        //    break;

                        default:
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                            break;
                    }

                    break;
            }
        }


        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                    {
                    new List<KeyboardButton>{  new KeyboardButton { Text = TEXT_1 }, new KeyboardButton {  Text = TEXT_2},  },
                    new List<KeyboardButton>{  new KeyboardButton {  Text = TEXT_3}, new KeyboardButton {  Text = TEXT_4},  }
                    },
                ResizeKeyboard = true
            };
        }

        private IReplyMarkup GetCategory()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                    {
                    new List<KeyboardButton>{  new KeyboardButton { Text = CAT_1 }, new KeyboardButton {  Text = CAT_2 },  },
                    new List<KeyboardButton>{  new KeyboardButton {  Text = CAT_3 }, new KeyboardButton {  Text = CAT_4 },  },
                    new List<KeyboardButton>{ new KeyboardButton { Text = CANCEL} }
                    },
                ResizeKeyboard = true
            };
        }

        private IReplyMarkup CancelButton()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                    {
                    new List<KeyboardButton>{ new KeyboardButton { Text = CANCEL} }
                    },
                ResizeKeyboard = true
            };
        }

        private int RandomNo()
        {
            Random rnd = new Random();
            int myRandomNo = rnd.Next(1000, 99999);
            return myRandomNo;
        }

        private void SendTlgChannel(string TicketType, string TicketCategory, int TicketNumber, string UserName, string PhoneNumber)
        {
            string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
            string apiToken = "1651450187:AAGzvWpcycfwKMZScYk6Og3tizd-zvgFjWc";
            string chatId = "@qvatro_tickets";
            string text = $"Заявка: №{TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}";
            urlString = String.Format(urlString, apiToken, chatId, text);
            WebRequest request = WebRequest.Create(urlString);
            Stream rs = request.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(rs);
            string line = "";
            StringBuilder sb = new StringBuilder();
            while (line != null)
            {
                line = reader.ReadLine();
                if (line != null)
                    sb.Append(line);
            }
            string response = sb.ToString();
        }

        //private void SendAsync(string TicketType, string TicketCategory, int TicketNumber, string UserName, string PhoneNumber)
        //{
            
        //}

        private void SendAsync(string TicketType, string TicketCategory, int TicketNumber, string UserName, string PhoneNumber)
        {
            try
            {
                var smtpClient = new SmtpClient("10.1.0.218")
                {
                    Port = 25,
                    //Credentials = new NetworkCredential("email", "password"),
                    EnableSsl = false,
                };

                smtpClient.Send("noreply@ntpayments.com", "afanasev@ntpayments.com", $"Заявка №{TicketNumber}", $"Заявка №: {TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}");
                Task.Delay(10000).Wait();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
    }
    }
}
