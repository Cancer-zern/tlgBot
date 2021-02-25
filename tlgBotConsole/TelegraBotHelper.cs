using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace tlgBotConsole
{
    internal class TelegraBotHelper
    {

        private const string MAIN_1 = "Установка";
        private const string MAIN_2 = "Проблема";
        private const string MAIN_3 = "Другое";
        private const string MAIN_4 = "Что-то еще";
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
                    var msgid = update.Message.Chat.Username;
                    var tlgusername = update.Message.Chat.Username;

                    if (tlgusername != inst.TlgUserName)
                    {
                        inst.TlgUserName = tlgusername;
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                    }
                    else
                    {

                        // TODO: need to code refactoring in the feature and build this app in Asp.Net
                        if (text == CANCEL)
                        {
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                            inst.Clear();
                            break;
                        }

                        if (inst.TicketType == MAIN_3 & string.IsNullOrEmpty(inst.TicketCategory) & string.IsNullOrEmpty(inst.UserName) & string.IsNullOrEmpty(inst.TicketDescription))
                        {
                            inst.TicketDescription = text;
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите ваше имя", replyMarkup: CancelButton());
                            break;
                        }

                        if (inst.TicketType == MAIN_3 & string.IsNullOrEmpty(inst.TicketCategory) & !string.IsNullOrEmpty(inst.TicketDescription) & string.IsNullOrEmpty(inst.PhoneNumber) & string.IsNullOrEmpty(inst.UserName) | !string.IsNullOrEmpty(inst.TicketType) & !string.IsNullOrEmpty(inst.TicketCategory) & string.IsNullOrEmpty(inst.UserName))
                        {
                            inst.UserName = text;
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона \nПример: 89281234567", replyMarkup: CancelButton());
                            break;
                        }

                        if (!string.IsNullOrEmpty(inst.TicketType) & !string.IsNullOrEmpty(inst.UserName))
                        {
                            //if (text.All(char.IsDigit) & text.Substring(0, 1) == "8" || text.Substring(0, 1) == "7" & text.Length == 11)
                            if (text.All(char.IsDigit) & text.Substring(0, 1) == "8" & text.Length == 11)
                            {
                                inst.PhoneNumber = text;
                                // create random no of ticket
                                inst.TicketNumber = RandomNo();
                                // just for cw
                                Console.WriteLine("ticket: " + inst.TicketNumber);
                                Console.WriteLine("type: " + inst.TicketType);
                                Console.WriteLine("category: " + inst.TicketCategory);
                                Console.WriteLine("description: " + inst.TicketDescription);
                                Console.WriteLine("username" + inst.UserName);
                                Console.WriteLine("phone: " + inst.PhoneNumber);
                                Console.WriteLine("tlg: " + inst.TlgUserName);
                                Console.WriteLine("------------");
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Спасибо, {inst.UserName}! Ваша заявка №{inst.TicketNumber} принята.");
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Перенаправляю вас в Главное меню!", replyMarkup: GetButtons());
                                // run method for sendind tlgChannel
                                SendTlgChannel(inst.TicketType, inst.TicketCategory, inst.TicketNumber, inst.TicketDescription, inst.UserName, inst.PhoneNumber, inst.TlgUserName);
                                // run method for sendind Email
                                //SendEmail(inst.TicketType, inst.TicketCategory, inst.TicketNumber, inst.UserName, inst.PhoneNumber);
                                inst.Clear();
                            }
                            else
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный номер, пожалуйста, введите правильный номер телефона \nПример: 89281234567", replyMarkup: CancelButton());
                            break;
                        }

                        switch (text)
                        {

                            case MAIN_1:
                            case MAIN_2:
                            case MAIN_4:
                                inst.TicketType = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите категорию в меню", replyMarkup: GetCategory());
                                break;
                            case MAIN_3:
                                inst.TicketType = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите описание проблемы", replyMarkup: CancelButton());
                                break;


                            case CAT_1:
                            case CAT_2:
                            case CAT_3:
                            case CAT_4:
                                inst.TicketCategory = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя", replyMarkup: CancelButton());
                                break;

                            default:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                                break;
                        }
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
                    new List<KeyboardButton>{  new KeyboardButton { Text = MAIN_1 }, new KeyboardButton {  Text = MAIN_2},  },
                    new List<KeyboardButton>{  new KeyboardButton {  Text = MAIN_3}, new KeyboardButton {  Text = MAIN_4},  }
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

        private async void SendTlgChannel(string TicketType, string TicketCategory, int TicketNumber, string TicketDescription, string UserName, string PhoneNumber, string TlgUserName)
        {
            //string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
            //string apiToken = "1651450187:AAGzvWpcycfwKMZScYk6Og3tizd-zvgFjWc";
            //string chatId = "@qvatro_tickets";
            //string text = $"Заявка: №{TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}";
            //urlString = String.Format(urlString, apiToken, chatId, text);
            //WebRequest request = WebRequest.Create(urlString);
            //Stream rs = request.GetResponse().GetResponseStream();
            //StreamReader reader = new StreamReader(rs);
            //string line = "";
            //StringBuilder sb = new StringBuilder();
            //while (line != null)
            //{
            //    line = reader.ReadLine();
            //    if (line != null)
            //        sb.Append(line);
            //}
            //string response = sb.ToString();

            var token_tlgchannel = "1651450187:AAGzvWpcycfwKMZScYk6Og3tizd-zvgFjWc";
            string text = $"Заявка: №{TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nОписание (опционально): {TicketDescription}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}\nТелеграм аккаунт: https://t.me/{TlgUserName}";
            var tlgbot = new TelegramBotClient(token_tlgchannel);
            var s = await tlgbot.SendTextMessageAsync("@qvatro_tickets", text);

        }


        private void SendEmail(string TicketType, string TicketCategory, int TicketNumber, string UserName, string PhoneNumber)
        {
            try
            {
                var smtpClient = new SmtpClient("10.1.0.218")
                {
                    Port = 25,
                    //Credentials = new NetworkCredential("email", "password"),
                    EnableSsl = false,
                };

                smtpClient.Send("noreply@xxxxx.com", "username@xxxxx.com", $"Заявка №{TicketNumber}", $"Заявка: № {TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}");
                Task.Delay(10000).Wait();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
        }
    }
}
