using System;
using System.Data.SqlClient;

namespace WordLoader
{
    public class DatabaseHelper
    {
        public string _connectionString = @"Data Source= DESKTOP-KSERBB9\MSSQLSER;Initial Catalog=WordLibrary;Integrated Security=True";

        public void CreateDatabaseAndSchema()
        {
            CreateDatabase();
            CreateSchema();
        }

        private void CreateDatabase()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WordLibrary')
                                    CREATE DATABASE WordLoaderDB";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CreateSchema()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'words')
                                    DROP TABLE words;
                                CREATE TABLE words (id INT PRIMARY KEY IDENTITY(1,1), word NVARCHAR(20) NOT NULL, count INT NOT NULL DEFAULT 0);";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}