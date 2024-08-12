using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace WordLoader
{
    public class WordLoader
    {
        private string _connectionString;

        public WordLoader(string connectionString)
        {
            _connectionString = connectionString;
        }
       
        public void InitializeDatabase()
        {
            // Бд не указывается потому что изначально она может быть не создана
            using var connection = new SqlConnection("Data Source=DESKTOP-KSERBB9\\MSSQLSER;Integrated Security=True");
            connection.Open();

            // Проверка бд
            string checkDatabaseQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WordDatabase')
                BEGIN
                    CREATE DATABASE WordDatabase;
                END";

            var command = new SqlCommand(checkDatabaseQuery, connection);
            command.ExecuteNonQuery();

            // Переключение на новую или существующую бд
            connection.ChangeDatabase("WordDatabase");

            // Создание таблицы, если она не существует
            string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Words')
                BEGIN
                    CREATE TABLE Words (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Word NVARCHAR(20) NOT NULL UNIQUE,
                        Count INT DEFAULT 0
                    );
                END";

            command.CommandText = createTableQuery;
            command.ExecuteNonQuery();
        }

        public Dictionary<string, int> LoadWordsFromFile(string filePath)
        {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var reader = new StreamReader(filePath, Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Регулярное выражение для поиска всех слов
                    var words = Regex.Matches(line, @"[a-zA-Zа-яА-ЯёЁ]{3,20}");

                    foreach (Match match in words)
                    {
                        string word = match.Value.Trim();
                        if (wordCounts.ContainsKey(word))
                        {
                            wordCounts[word]++;
                        }
                        else
                        {
                            wordCounts[word] = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
            }
                       
            // Удаления слов которые повторяются меньше 4 раз
            var filteredWordCounts = wordCounts
                .Where(pair => pair.Value >= 4)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return filteredWordCounts;
        }

        public void InsertOrUpdateWordsInDatabase(Dictionary<string, int> wordCounts)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            connection.ChangeDatabase("WordDatabase");

            using var transaction = connection.BeginTransaction();
            SqlCommand command = connection.CreateCommand();
            command.Transaction = transaction;

            int totalInserted = 0; // Колво загруженных слов

            foreach (var pair in wordCounts)
            {
                command.CommandText = @"
            MERGE INTO Words AS target
            USING (SELECT @Word AS Word, @Count AS Count) AS source
            ON target.Word = source.Word
            WHEN MATCHED THEN
                UPDATE SET target.Count = target.Count + source.Count
            WHEN NOT MATCHED THEN
                INSERT (Word, Count) VALUES (source.Word, source.Count);";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Word", pair.Key);
                command.Parameters.AddWithValue("@Count", pair.Value);

                totalInserted += command.ExecuteNonQuery();
            }

            transaction.Commit();
            
            Console.WriteLine($"Успешно загружено {totalInserted} слов(а) в базу данных.");
        }
    }
}