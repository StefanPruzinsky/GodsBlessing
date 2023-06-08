using System;
using System.Collections.Generic;
using System.Linq;

using MySql.Data.MySqlClient;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace GodsBlessing.DatabaseLibrary
{
    public enum DatabaseType
    {
        MySQL,
        SQLite
    }

    class DatabaseHelper
    {
        public Database Database { get; private set; }
        public APIHelper APIHelper { get; private set; }

        public bool WasConfiguredCorrectly { get; private set; }

        private Database silentDatabase;

        public DatabaseHelper(DatabaseType databaseType)
        {
            silentDatabase = new SQLiteDatabase(BuildSQLiteConnectionString());
            WasConfiguredCorrectly = silentDatabase.Connect();

            if (databaseType == DatabaseType.MySQL && WasConfiguredCorrectly)
            {
                Database = silentDatabase;

                APIHelper = new APIHelper(Select("settings", new string[]{ "value" }, "name", "apiKey").ToString(), Select("settings", new string[] { "value" }, "name", "serverPath").ToString());

                Database = new MySQLDatabase(BuildMySQLConnectionString());
                WasConfiguredCorrectly = Database.Connect();
            }
            else if (databaseType == DatabaseType.SQLite)
                Database = silentDatabase;
        }

        public void Insert(string tableName, string[] columnsNames, string[] values)
        {
            Database.Query(String.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, String.Join(", ", columnsNames), String.Join(", ", values.Select(s => String.Format("'{0}'", s)).ToArray())));
        }

        public object Select(string tableName, string[] columnsNames)
        {
            return Database.Query(String.Format("SELECT {0} FROM {1}", String.Join(", ", columnsNames), tableName));
        }

        public object Select(string tableName, string[] columnsNames, string conditionColumn, string conditionValue)
        {
            return Database.Query(String.Format("SELECT {0} FROM {1} WHERE {2}='{3}'", String.Join(", ", columnsNames), tableName, conditionColumn, conditionValue));
        }

        public void Update(string tableName, string[] columnsNames, string[] values, string conditionColumn, string conditionValue)
        {
            Database.Query(String.Format("UPDATE {0} SET {1} WHERE {2}='{3}'", tableName, String.Join(", ", columnsNames.Select((s, index) => String.Format("{0}='{1}'", s, values[index]))), conditionColumn, conditionValue));
        }

        public void CreateTable(string tableName, dynamic tableTemplate)
        {
            List<string> processedTableTemplate = new List<string>();

            foreach (JProperty columnDefinition in tableTemplate)
                processedTableTemplate.Add(String.Format("{0} {1}", columnDefinition.Name, columnDefinition.Value));

            Database.Query(String.Format("CREATE TABLE IF NOT EXISTS {0} ( {1} )", tableName, String.Join(",", processedTableTemplate)));
        }

        private string BuildMySQLConnectionString()
        {
            dynamic databaseSettings = APIHelper.GetData("DatabaseConfiguration", "databaseSettings").response;

            MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
            mySqlConnectionStringBuilder.Server = databaseSettings.hostName;
            mySqlConnectionStringBuilder.Database = databaseSettings.databaseName;
            mySqlConnectionStringBuilder.UserID = databaseSettings.userName;
            mySqlConnectionStringBuilder.Password = databaseSettings.password;

            return mySqlConnectionStringBuilder.ConnectionString;
        }

        private string BuildSQLiteConnectionString()
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder();
            sqliteConnectionStringBuilder.DataSource = "GodsBlessing.db";

            return sqliteConnectionStringBuilder.ConnectionString;
        }
    }
}
