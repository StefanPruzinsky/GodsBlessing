using System;

using Microsoft.Data.Sqlite;

namespace GodsBlessing.DatabaseLibrary
{
    class SQLiteDatabase: Database
    {
        public SQLiteDatabase(string connectionString): base(connectionString) { }

        public override bool Connect()
        {
            bool connectionResult = true;

            try
            {
                sqlConnection = new SqliteConnection(connectionString);
                sqlConnection.Open();
            }
            catch
            {
                connectionResult = false;
            }

            return connectionResult;
        }

        public override object Query(string query)
        {
            SqliteCommand sqliteCommand = new SqliteCommand(query, sqlConnection);

            return sqliteCommand.ExecuteScalar(); //ExecuteReader() gets all requested results. ExecuteScalar() just first one.
        }
    }
}
