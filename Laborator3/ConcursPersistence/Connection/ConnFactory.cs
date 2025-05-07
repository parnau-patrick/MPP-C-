using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ConcursPersistence.Connection
{
    public abstract class ConnFactory
    {
        protected ConnFactory()
        {
            
        }
        
        private static ConnFactory? instance;

        public static ConnFactory GetInstance()
        {
            if (instance == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(ConnFactory)))
                    {
                        instance = (ConnFactory)Activator.CreateInstance(type);
                    }
                }
            }
            return instance!;
        }
        
        public abstract IDbConnection CreateConnection(IDictionary<string, string> properties);
    }
}