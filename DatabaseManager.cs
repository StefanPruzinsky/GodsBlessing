using System;
using System.Collections.Generic;
using System.Linq;

using GodsBlessing.DatabaseLibrary;

namespace GodsBlessing
{
    class DatabaseManager
    {
        public bool InitializationExecution { get; set; }
        public string VersionOfStuff { get; private set; }
        public string APIKey { get; private set; }
        public string ServerPath { get; private set; }

        public string TerminalHeader { get; private set; }
        public string TerminalCommandsList { get; private set; }

        private DatabaseHelper sqliteDatabaseHelper;
        private DatabaseHelper mySQLDatabaseHelper;

        public DatabaseManager()
        {
            InitializeDatabaseHelper(DatabaseType.SQLite);

            InitializationExecution = bool.Parse(GetGodsBlessingSetting("wasInitialized"));

            if (InitializationExecution)
                InitializeDatabaseHelper(DatabaseType.MySQL);

            VersionOfStuff = GetGodsBlessingSetting("versionOfStuff");
            APIKey = GetGodsBlessingSetting("apiKey");
            ServerPath = GetGodsBlessingSetting("serverPath");

            TerminalHeader = GetGodsBlessingText("terminalHeader").Replace("##version##", VersionOfStuff);
            TerminalCommandsList = GetGodsBlessingText("terminalCommandsList");
        }

        public void InitializeDatabaseHelper(DatabaseType databaseType)
        {
            bool wasConfiguredCorrectly = false;

            if (databaseType == DatabaseType.SQLite)
            {
                sqliteDatabaseHelper = new DatabaseHelper(DatabaseType.SQLite);
                wasConfiguredCorrectly = sqliteDatabaseHelper.WasConfiguredCorrectly;

                if (!wasConfiguredCorrectly)
                    throw new Exception("Database connection wasn't succeed. Please, try to check your database file whether it's not moved/renamed/deleted.");
            }
            else if (databaseType == DatabaseType.MySQL)
            {
                mySQLDatabaseHelper = new DatabaseHelper(DatabaseType.MySQL);
                wasConfiguredCorrectly = mySQLDatabaseHelper.WasConfiguredCorrectly;

                if (!wasConfiguredCorrectly)
                    throw new Exception("Database connection wasn't succeed. Please, reenter database credentials in DatabaseConfiguration file.");
            }
        }

        public bool TryDatabaseConnection(DatabaseType databaseType)
        {
            bool result = true;

            try
            {
                InitializeDatabaseHelper(databaseType);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        //SQLite database tasks
        public string GetGodsBlessingSetting(string nameOfSetting)
        {
            return sqliteDatabaseHelper.Select("settings", new string[] { "value" }, "name", nameOfSetting).ToString();
        }

        public void SetGodsBlessingSetting(string nameOfSetting, string value)
        {
            sqliteDatabaseHelper.Update("settings", new string[] { "value" }, new string[] { value }, "name", nameOfSetting);
        }

        public string GetGodsBlessingText(string nameOfText)
        {
            return sqliteDatabaseHelper.Select("godsBlessingTexts", new string[] { "value" }, "name", nameOfText).ToString();
        }

        public string GetStuffScript(string nameOfScript)
        {
            return sqliteDatabaseHelper.Select("stuffScripts", new string[] { "code" }, "nameOfCode", nameOfScript).ToString();
        }

        public string GetUserManagementScript(string nameOfScript)
        {
            return sqliteDatabaseHelper.Select("userManagementScripts", new string[] { "code" }, "nameOfCode", nameOfScript).ToString();
        }

        //MySQL database tasks
        public void CreateTable(string tableName, dynamic tableTemplate)
        {
            mySQLDatabaseHelper.CreateTable(tableName, tableTemplate);
        }

        public void CreateNewUser(Dictionary<string, string> userData)
        {
            mySQLDatabaseHelper.Insert("users", userData.Keys.ToArray(), userData.Values.ToArray());
        }
    }
}
