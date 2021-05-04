using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace HomeWork9
{
    internal class Program
    {
        public static TelegramBotClient bot;

        static void Main(string[] args)
        {
            string token = File.ReadAllText(@"../../../../../token.txt");

            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.StartReceiving();
            Console.ReadKey();
        }
        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");
 
            

            if (e.Message.Text == null) return;

            var messageText = e.Message.Text;

            
            bot.SendTextMessageAsync(e.Message.Chat.Id,
                $"{messageText}"
            );
        }
    }
}