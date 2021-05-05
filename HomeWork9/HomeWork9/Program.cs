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
            string token = File.ReadAllText(@"../../../../../token.txt");

            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.StartReceiving();
            Console.ReadKey();
        }
        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {e.Message.Type}");

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
            }

            if (e.Message.Text == @"/Download")
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(@"Download");
                var namesFile = directoryInfo.GetFiles();

                string messageFile = "Выбери файл для скачивания:\n";

                for (int i = 0; i < namesFile.Length; i++)
                {
                    messageFile += $"{i + 1} - {namesFile[i].Name}\n";
                }

                bot.SendTextMessageAsync(e.Message.Chat.Id, messageFile);
            }
            
            if (e.Message.Text == null) return;

            var messageText = e.Message.Text;

            
            bot.SendTextMessageAsync(e.Message.Chat.Id,
                $"{messageText}"
            );
        }
        
        static async void DownLoad(string fileId, string path)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream("_" + path, FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
             
            fs.Dispose();
        }

        static async void SendFile(ChatId chatId, string path)
        {
            using (var fs = File.OpenRead(path))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                iof.FileName = "Document.pdf";

                var send = await bot.SendDocumentAsync(chatId, iof);
            }
        }
    }
}