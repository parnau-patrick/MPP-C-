using System;
using System.Collections.Generic;
using System.Data;
using ConcursPersistence.Connection;
using log4net;

namespace ConcursPersistence.utils
{
    public class DBUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DBUtils));
        private static IDbConnection? instance = null;

        public static IDbConnection GetConnection(IDictionary<string, string> props)
        {
            if (instance == null || instance.State == ConnectionState.Closed)
            {
                instance = GetNewConnection(props);
                instance.Open();
            }
            return instance;
        }

        private static IDbConnection GetNewConnection(IDictionary<string, string> props)
        {
            ConnFactory factory = ConnFactory.GetInstance();
            return factory.CreateConnection(props);
        }
    }
}