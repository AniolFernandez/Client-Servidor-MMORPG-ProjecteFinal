using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
public class DatabaseConnection
{
    private DatabaseConnection()
    {
    }

    private string databaseName = string.Empty;
    public string DatabaseName
    {
        get { return databaseName; }
        set { databaseName = value; }
    }
    public string DB_IP { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    private MySqlConnection connection = null;
    public MySqlConnection Connection
    {
        get { return connection; }
    }

    private static DatabaseConnection _instance = null;
    public static DatabaseConnection Instance()
    {
        if (_instance == null)
            _instance = new DatabaseConnection();
        return _instance;
    }

    public bool IsConnect()
    {
        try
        {
            if (Connection == null)
            {
                if (System.String.IsNullOrEmpty(databaseName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", DB_IP, databaseName, User, Password);
                connection = new MySqlConnection(connstring);
                connection.Open();
            }
            else
                connection.Open();
        }
        catch
        {
            return false;
        }
        return true;
    }

    public void Close()
    {
        try
        {
            connection.Close();
        }
        catch { }
    }
}
