using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace HomeWork9
{
    internal class Program
    {
        public static TelegramBotClient bot;

        static void Main(string[] args)
        {
            string token = "";

            MyTelegramBot telegramBot = new MyTelegramBot(token);

            telegramBot.bot.OnMessage += telegramBot.TakeMessage;
            telegramBot.bot.StartReceiving();
            Console.ReadKey();
        }
    }
}