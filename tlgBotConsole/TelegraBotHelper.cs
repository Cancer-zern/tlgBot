﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
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
        private const string CAT_3 = "Сервер";
        private const string CAT_4 = "1С Предприятие";

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

                    Thread.Sleep(1000);
                }
            }
        }

        //private static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        //{
        //    _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя", replyMarkup: new ReplyKeyboardRemove());
        //}

            EmailForm inst = new EmailForm();
        private void processUpdate(Telegram.Bot.Types.Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    //var username1 = _client.GetMeAsync().Result;
                    //var chat_user_client = update.Message.Contact.UserId;
                    //var c = update.Message.ReplyToMessage;
                    //string imagePath = null;
                    if (inst.TicketType != null & inst.TicketCategory != null & inst.UserName == null)
                    {
                        inst.UserName = text;
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона (пример 89281234567)", replyMarkup: new ReplyKeyboardRemove());
                        break;
                    }

                    if (inst.TicketType != null & inst.TicketCategory != null & inst.UserName != null)
                    {
                        inst.PhoneNumber = text;
                        inst.TicketNumber = RandomNo();
                        Console.WriteLine(inst.TicketNumber);
                        Console.WriteLine(inst.TicketType);
                        Console.WriteLine(inst.TicketCategory);
                        Console.WriteLine(inst.UserName);
                        Console.WriteLine(inst.PhoneNumber);
                        _client.SendTextMessageAsync(update.Message.Chat.Id, $"Спасибо, {inst.UserName}! Ваша заявка №{inst.TicketNumber} принята.", replyMarkup: new ReplyKeyboardRemove());
                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Перенаправляю Вас в Главное меню!", replyMarkup: GetButtons());
                        SendEmail(inst.TicketType, inst.TicketCategory, inst.TicketNumber, inst.UserName, inst.PhoneNumber);
                        inst.Clear();
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
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя", replyMarkup: new ReplyKeyboardRemove());
                            break;


                        default:
                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите пункт в меню", replyMarkup: GetButtons());
                            break;
                    }

                    break;
            }
        }


                    //        case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                    //            switch (update.CallbackQuery.Data)
                    //            {
                    //                case "1":
                    //                    var msg1 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ `{update.CallbackQuery.Data}` принят", replyMarkup: GetButtons()).Result;
                    //                    break;
                    //                case "2":
                    //                    var msg2 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ `{update.CallbackQuery.Data}` принят", replyMarkup: GetButtons()).Result;
                    //                    break;
                    //                case "3":
                    //                    var msg3 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ `{update.CallbackQuery.Data}` принят", replyMarkup: GetButtons()).Result;
                    //                    break;
                    //                case "4":
                    //                    var msg4 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ `{update.CallbackQuery.Data}` принят", replyMarkup: GetButtons()).Result;
                    //                    break;
                    //            }
                    //            break;
                    //        default:
                    //            Console.WriteLine(update.Type + " Not ipmlemented!");
                    //            break;
                    //    }
                    //}



                    
        //private void processUpdate(Telegram.Bot.Types.Update update)
        //{
            
        //    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        //    {
        //        var text = update.Message.Text;
        //        //inst.TicketType = text;
        //        //inst.TicketCategory = update.Message.Text;
        //        if (text != "Установка")
        //        {
        //            _client.SendTextMessageAsync(update.Message.Chat.Id, "Choose section into menu", replyMarkup: GetButtons());
        //        }
        //        //else
        //        //{
        //        //    inst.TicketType = text;
        //        //}

        //        //Console.WriteLine(inst.TicketType);
        //        if (text == "Установка")
        //        {
        //            //Console.WriteLine(inst.TicketType);
        //            inst.TicketType = text;
        //            _client.SendTextMessageAsync(update.Message.Chat.Id, "Choose the category", replyMarkup: GetCategory());
        //        }

        //        if (text == "Камеры")
        //        {
        //            inst.TicketCategory = text;
        //            _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите имя", replyMarkup: new ReplyKeyboardRemove());
        //            //_client.SendTextMessageAsync(callback.Message.Chat.Id);
        //        }

        //        if (true)
        //        {

        //        }
        //        Console.WriteLine(inst.TicketType + " last-1");
        //        Console.WriteLine(inst.TicketCategory + " last-2");

        //    }
        
        //}





        private IReplyMarkup GetInlineButton(int id)
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "Заказать", CallbackData = id.ToString() });
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
                    new List<KeyboardButton>{  new KeyboardButton {  Text = CAT_3 }, new KeyboardButton {  Text = CAT_4 },  }
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
        private void SendEmail(string TicketType, string TicketCategory, int TicketNumber, string UserName, string PhoneNumber)
        {
            var smtpClient = new SmtpClient("10.1.0.218")
            {
                Port = 25,
                //Credentials = new NetworkCredential("email", "password"),
                EnableSsl = false,
            };

            smtpClient.Send("noreply@ntpayments.com", "afanasev@ntpayments.com", $"Заявка №{TicketNumber}", $"Заявка №: {TicketNumber}\nТип заявки: {TicketType}\nКатегория: {TicketCategory}\nИмя пользователя: {UserName}\nНомер телефона: {PhoneNumber}");

        }
    }
}
