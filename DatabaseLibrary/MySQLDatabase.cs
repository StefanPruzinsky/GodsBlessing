using System;

using MySql.Data.MySqlClient;

namespace GodsBlessing.DatabaseLibrary
{
    class MySQLDatabase: Database
    {
        public MySQLDatabase(string connectionString): base(connectionString) { }

        public override bool Connect()
        {
            bool connectionResult = true;

            try
            {
                sqlConnection = new MySqlConnection(connectionString);
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
            MySqlCommand mySqlCommand = new MySqlCommand(query, sqlConnection);

            return mySqlCommand.ExecuteScalar(); //ExecuteReader() gets all requested results. ExucuteScalar() just first one.
        }
    }
}
