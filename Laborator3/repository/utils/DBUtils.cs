using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using log4net;

namespace Laborator3.repository_utils
{
    public class DBUtils
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static IDbConnection instance = null;

        public static IDbConnection getConnection(IDictionary<string, string> props)
        {
            if (instance == null || instance.State == ConnectionState.Closed)
            {
                instance = getNewConnection(props);
                instance.Open();
            }
            return instance;
        }

        private static IDbConnection getNewConnection(IDictionary<string, string> props)
        {
            string connectionString = props["ConnectionString"];
            log.Info("Opening database connection with connection string: " + connectionString);
            return new SQLiteConnection(connectionString);
        }
    }
}