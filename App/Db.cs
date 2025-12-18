using MySqlConnector;

namespace ESILV_A2_S1_ConceptionBD.App;

public static class Db
{
    public static async Task<MySqlConnection> OpenAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
