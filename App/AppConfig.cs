using Microsoft.Extensions.Configuration;

namespace ESILV_A2_S1_ConceptionBD.App;

public sealed record DatabaseConnections(
    string AdminPrincipal,
    string AdminSecondaire,
    string Membre,
    string Reporting);

public sealed record DatabaseConfig(
    string Name,
    string Salt,
    DatabaseConnections Connections);

public static class AppConfig
{
    public static DatabaseConfig LoadDatabaseConfig()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        DatabaseConfig? db = config.GetSection("Database").Get<DatabaseConfig>();
        if (db is null)
        {
            throw new InvalidOperationException("Missing 'Database' section in appsettings.json");
        }

        if (string.IsNullOrWhiteSpace(db.Salt))
        {
            throw new InvalidOperationException("Missing Database.Salt in appsettings.json");
        }

        return db;
    }
}
