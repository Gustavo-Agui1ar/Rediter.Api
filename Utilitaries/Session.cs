using Npgsql;
using System.Data;

public class Session
{
    private readonly string _connectionString;
    private NpgsqlConnection _connection { get; set; } = null!;

    public Session(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        CreateConnection();
    }

    ~Session()
    {
        CloseConnection();
    }

    private void CreateConnection()
    {
        _connection = new NpgsqlConnection(_connectionString);
        _connection.Open();
    }

    private void EnsureConnection()
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }
    }

    private void CloseConnection()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }
    }

    public T Execute<T>(Func<NpgsqlConnection, T> action)
    {
        EnsureConnection();
        return action(_connection);
    }
}