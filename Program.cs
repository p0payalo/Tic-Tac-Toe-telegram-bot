using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using System.Runtime.Serialization.Formatters.Binary;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Requests;
using TicTacToeTG.Core.Types;
using TicTacToeTG.Core;
using System.Runtime.Remoting.Messaging;

namespace TicTacToeTG
{
    class Program
    {
        static TelegramBotClient client;
        static BotHandler handler;
        static string token;

        static void Main(string[] args)
        {
            using(StreamReader sr = new StreamReader("token.txt"))
            {
                token = sr.ReadLine();
            }
            client = new TelegramBotClient(token);
            client.OnMessage += OnMessage;
            client.OnCallbackQuery += OnCallback;
            List<BotUser> users = new List<BotUser>();
            if(System.IO.File.Exists("users.dat"))
            {
                using (FileStream fs = new FileStream("users.dat", FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    users = (List<BotUser>)bf.Deserialize(fs);
                }
            }
            handler = new BotHandler(client, users);
            client.StartReceiving();
            Console.ReadLine();
        }

        public static async void OnMessage(object sender, MessageEventArgs args)
        {
            Message msg = args.Message;
            BotUser us = handler.users.Find(x => x.chatId == msg.Chat.Id);
            if (us == null)
            {
                us = new BotUser()
                {
                    chatId = msg.Chat.Id,
                    username = msg.From.Username,
                    step = 0,
                    targetCommand = "",
                    currentGame = null,
                };
                handler.users.Add(us);
                using (FileStream fs = new FileStream("users.dat", FileMode.OpenOrCreate))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, handler.users);
                }
            }
            //update username if user change it
            us.username = msg.From.Username;
            try
            {
                handler.ExecuteCommandAsync(msg, us);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static async void OnCallback(object sender, CallbackQueryEventArgs args)
        {
            BotUser us = handler.users.Find(x => x.chatId == args.CallbackQuery.Message.Chat.Id);
            handler.HandleCallbackAsync(args.CallbackQuery, us);
        }
    }
}
