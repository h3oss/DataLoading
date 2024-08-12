using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Text;

namespace WordLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            // Файл со словами
            string filePath = @"C:\Users\Heos\Desktop\Test\DataLoading\DataLoading\Words.txt";

            // Подключение к бд
            string connectionString = @"Data Source= DESKTOP-KSERBB9\MSSQLSER;Initial Catalog=WordDatabase;Integrated Security=True";

            WordLoader loader = new WordLoader(connectionString);

            // Инициализация бд
            loader.InitializeDatabase();

            // Загрузка слов из файла
            var wordCounts = loader.LoadWordsFromFile(filePath);

            // Вставка или обновление слов в базе данных
            loader.InsertOrUpdateWordsInDatabase(wordCounts);
        }
    }

}