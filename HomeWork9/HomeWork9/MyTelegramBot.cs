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
        /// Отправить сообщение на команду /Start
        /// </summary>
        /// <param name="messageEvent"></param>
        public void SendStartMessage(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            string textMessage = $"Вот что я могу:\n" +
                                 $"1) Сохраняю любой отправленный файл, аудиосообщение или фото\n" +
                                 $"2) На команду /Download отправляю тебе любой сохраненный файл\n" +
                                 $"3) Обычные сообщения я просто повторяю\n" +
                                 $"Давай начнем!\n";

            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, textMessage);
        }
        
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
        public async void SendDocument(ChatId chatId, string path, string fileName)
        {
            using (var fs = File.OpenRead(path))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                iof.FileName = fileName;

                var send = await bot.SendDocumentAsync(chatId, iof);
            }
        }

        /// <summary>
        /// Отправить аудиосообщение с диска
        /// </summary>
        /// <param name="chatId">ID чата</param>
        /// <param name="fileName">Имя файла</param>
        public async void SendVoice(ChatId chatId, string path)
        {
            using (var fs = File.OpenRead(path))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                var send = await bot.SendVoiceAsync(chatId, iof);
            }
        }

        /// <summary>
        /// Отправить фото с диска
        /// </summary>
        /// <param name="chatId">ID чата</param>
        /// <param name="fileName">Имя файла</param>
        public async void SendPhoto(ChatId chatId, string path)
        {
            using (var fs = File.OpenRead(path))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                var send = await bot.SendPhotoAsync(chatId, iof);
            }
        }

        /// <summary>
        /// Выбрать тип файла
        /// </summary>
        public void SelectTypeFile(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            string sendMessage = "Выбери тип файла:\n" +
                                 "1 - Документ\n" +
                                 "2 - Аудиосообщение\n" +
                                 "3 - Фото";

            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, sendMessage);
        }

        /// <summary>
        /// Проверка типа файла
        /// </summary>
        /// <param name="messageEvent"></param>
        public void CheckTypeFile(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            int answer;
            
            if (Int32.TryParse(messageEvent.Message.Text, out answer) && answer > 0 && answer < 4)
            {
                switch (answer)
                {
                    case 1:
                        DownloadFolder += @"\Document";
                        break;
                    case 2:
                        DownloadFolder += @"\Voice";
                        break;
                    case 3:
                        DownloadFolder += @"\Photo";
                        break;
                }

                SelectFile(messageEvent);
            }
            else
            {
                bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Такого типа файла нет(");
                isDownload = false;
            }
        }
        
        /// <summary>
        /// Выбор файла для скачивания
        /// </summary>
        /// <param name="messageEvent"></param>
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
                isDownload = false;
                DownloadFolder = @"Download";
            }

            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, messageFile);
        }

        /// <summary>
        /// Проверка выбранного файла
        /// </summary>
        /// <param name="messageEvent"></param>
        public void CheckFile(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            int answer;
            
            DirectoryInfo directoryInfo = new DirectoryInfo(DownloadFolder);
            var files = directoryInfo.GetFiles();

            if (Int32.TryParse(messageEvent.Message.Text, out answer) && answer > 0 && answer <= files.Length)
            {      
                switch (directoryInfo.Name)
                {
                    case "Document":
                        SendDocument(messageEvent.Message.Chat.Id, files[answer - 1].FullName, files[answer - 1].Name);
                        break;
                    case "Photo":
                        SendPhoto(messageEvent.Message.Chat.Id, files[answer - 1].FullName);
                        break;
                    case "Voice":
                        SendVoice(messageEvent.Message.Chat.Id, files[answer - 1].FullName);
                        break;
                }
            }
            else
            {
                bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Такого файла нет(");
            }

            isDownload = false;
            DownloadFolder = @"Download";
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

        /// <summary>
        /// Повторить сообщение
        /// </summary>
        /// <param name="messageEvent"></param>
        public void RepeaterMessage(Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            if (messageEvent.Message.Text == null) return;

            var messageText = messageEvent.Message.Text;


            bot.SendTextMessageAsync(messageEvent.Message.Chat.Id,
                $"{messageText}"
            );
        }
                
        /// <summary>
        /// Реакция на новое сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEvent"></param>
        public void TakeMessage(object sender, Telegram.Bot.Args.MessageEventArgs messageEvent)
        {
            PrintInfoMessage(messageEvent);

            if (messageEvent.Message.Text == @"/start" || messageEvent.Message.Text == @"/Start")
            {
                SendStartMessage(messageEvent);
                return;
            }

            if (messageEvent.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                DownLoadFile(messageEvent.Message.Document.FileId, @"Document/" + messageEvent.Message.Document.FileName);
                bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Я скачал данный файл");
                return;
            }

            if (messageEvent.Message.Type == Telegram.Bot.Types.Enums.MessageType.Voice)
            {
                DownLoadFile(messageEvent.Message.Voice.FileId, $@"Voice/{DateTime.Now.ToString("yyyy.MM.dd HH_mm_ss")}.ogg");
                bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Я скачал данное аудиосообщение");
                return;
            }

            if (messageEvent.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                DownLoadFile(messageEvent.Message.Photo[messageEvent.Message.Photo.Length - 1].FileId, $@"Photo/{DateTime.Now.ToString("yyyy.MM.dd HH_mm_ss")}.jpg");
                bot.SendTextMessageAsync(messageEvent.Message.Chat.Id, "Я скачал данное фото");
                return;
            }

            if (isDownload)
            {
                if (DownloadFolder == @"Download")
                {
                    CheckTypeFile(messageEvent);
                }
                else
                {
                    CheckFile(messageEvent);
                }
                return;
            }

            if (messageEvent.Message.Text == @"/Download")
            {
                isDownload = true;
                SelectTypeFile(messageEvent);
                return;
            }
            
            RepeaterMessage(messageEvent);
        }

        /// <summary>
        /// Запустить бота
        /// </summary>
        public void StartBot()
        {
            bot.OnMessage += TakeMessage;
            bot.StartReceiving();
        }
        
        #endregion
    }
}
