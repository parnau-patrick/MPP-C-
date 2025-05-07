using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace ConcursPersistence.Connection
{
    public class SqliteConnFactory : ConnFactory
    {
        public override IDbConnection CreateConnection(IDictionary<string, string> properties)
        {
            string connectionString = properties["ConnectionString"];
            return new SQLiteConnection(connectionString);
        }
    }
}