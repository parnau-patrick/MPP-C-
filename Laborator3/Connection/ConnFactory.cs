namespace Laborator3.Connection;

using System.Data;
using System.Reflection;
public abstract class ConnFactory
{
    protected ConnFactory()
    {
        
    }
    
    private static ConnFactory instance;

    public static ConnFactory getInstance()
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
        return instance;
    }
    
    public abstract IDbConnection createConnection(IDictionary<string, string> properties);
}