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
        public async void DownLoad(string fileId, string fileName)
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
        public async void SendFile(ChatId chatId, string fileName)
        {
            using (var fs = File.OpenRead($@"{DownloadFolder}/{fileName}"))
            {
                Telegram.Bot.Types.InputFiles.InputOnlineFile iof =
                    new Telegram.Bot.Types.InputFiles.InputOnlineFile(fs);

                iof.FileName = fileName;

                var send = await bot.SendDocumentAsync(chatId, iof);
            }
        }

        #endregion
    }
}
