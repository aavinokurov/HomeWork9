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
    class MyTelegramBot
    {
        #region Поля

        /// <summary>
        /// Токен для создания бота
        /// </summary>
        private string token;

        /// <summary>
        /// Режим работы бота
        /// </summary>
        private bool isDownload;

        #endregion

        #region Свойства

        /// <summary>
        /// Телеграм бот клиент
        /// </summary>
        public TelegramBotClient bot { get; private set; }

        /// <summary>
        /// Папка для скачивания
        /// </summary>
        public static string DownloadFolder { get; private set; } = @"Download";

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создать телеграм бота
        /// </summary>
        /// <param name="token">Токен для подключения</param>
        /// <param name="downloadFolder">Папка для скачивания файлов</param>
        public MyTelegramBot(string token)
        {
            this.bot = new TelegramBotClient(token);
        }

        #endregion

        #region Методы

        /// <summary>
        /// Скачать файл на диск
        /// </summary>
        /// <param name="fileId">Id файла</param>
        /// <param name="fileName">Имя файла</param>
        public async void DownLoadFile(string fileId, string fileName)
        {
            var file = await bot.GetFileAsync(fileId);
            FileStream fs = new FileStream($@"{DownloadFolder}/{fileName}", FileMode.Create);
            await bot.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
            fs.Dispose();
        }

        /// <summary>
        /// Отправить файл с диска
        /// </summary>
        /// <param name="chatId">ID чата</param>
        /// <param name="fileName">Имя файла</param>
        public async void SendFile(ChatId chatId, string path)
        {
            using (var fs = File.OpenRead(path))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                var send = await bot.SendVoiceAsync(chatId, iof);
            }
        }

        /// <summary>
        /// Выбрать тип файл
        /// </summary>
        public void SelectTypeFile(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            string sendMessage = "Выбери тип файла:\n" +
                                 "1 - Документ\n" +
                                 "2 - Аудиосообщение";

            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, sendMessage);
        }

        public void SelectFile(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(DownloadFolder);
            var namesFile = directoryInfo.GetFiles();
            string messageFile;

            if (namesFile.Length > 0)
            {
                messageFile = "Выбери файл для скачивания:\n";

                for (int i = 0; i < namesFile.Length; i++)
                {
                    messageFile += $"{i + 1} - {namesFile[i].Name}\n";
                }
            }
            else
            {
                messageFile = "Файлов для скачивания нет.";
            }

            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, messageFile);
        }

        /// <summary>
        /// Реакция на новое сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEvent"></param>
        public void TakeMessage(object sender, Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            PrintInfoMessage(messageEvent);

            if (messageEvent.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                DownLoadFile(messageEvent.Message.Document.FileId, @"Document/" + messageEvent.Message.Document.FileName);
            }

            if (messageEvent.Message.Type == Telegram.Bot.Types.Enums.MessageType.Voice)
            {
                DownLoadFile(messageEvent.Message.Voice.FileId, @"Voice/" + DateTime.Now.ToString("yyyy.MM.dd HH_mm") + ".ogg");
            }

            if (isDownload)
            {
                int answer;

                if (DownloadFolder == @"Download")
                {
                    if (Int32.TryParse(messageEvent.Message.Text, out answer) && answer > 0 && answer < 3)
                    {
                        switch (answer)
                        {
                            case 1:
                                DownloadFolder += @"\Document";
                                break;
                            case 2:
                                DownloadFolder += @"\Voice";
                                break;
                        }

                        SelectFile(messageEvent);
                    }
                    else
                    {
                        bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Такого типа файла нет.");
                        isDownload = false;
                    }
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(DownloadFolder);
                    var files = directoryInfo.GetFiles();

                    if (Int32.TryParse(messageEvent.Message.Text, out answer) && answer > 0 && answer <= files.Length)
                    {      
                        SendFile(messageEvent.Message.Chat.Id, files[answer - 1].FullName);
                    }
                    isDownload = false;
                    DownloadFolder = @"Download";
                }
            }

            if (messageEvent.Message.Text == @"/Download")
            {
                isDownload = true;
                SelectTypeFile(messageEvent);
            }

            if (messageEvent.Message.Text == null) return;

            var messageText = messageEvent.Message.Text;


            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id,
                $"{messageText}"
            );
        }

        /// <summary>
        /// Вывод в консоль информацию о сообщении
        /// </summary>
        /// <param name="messageEvent"></param>
        public void PrintInfoMessage(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {messageEvent.Message.Chat.FirstName} {messageEvent.Message.Chat.Id} {messageEvent.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {messageEvent.Message.Type}");
        }

        #endregion
    }
}
