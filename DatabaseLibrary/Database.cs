using System;
using System.Collections.Generic;
using System.Text;

namespace GodsBlessing.DatabaseLibrary
{
    abstract class Database
    {
        protected dynamic sqlConnection;

        protected string connectionString;

        protected Database(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public abstract bool Connect();

        public abstract object Query(string query);
    }
}
