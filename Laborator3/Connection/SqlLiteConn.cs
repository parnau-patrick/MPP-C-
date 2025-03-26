namespace Laborator3.Connection;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

public class SqlLiteConnFactory : ConnFactory
{
    public override IDbConnection createConnection(IDictionary<string, string> properties)
    {
        string connectionString = properties["connectionString"];
      
        
        return new SQLiteConnection(connectionString);
    }
}