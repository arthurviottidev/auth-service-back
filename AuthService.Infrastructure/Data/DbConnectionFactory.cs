using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AuthService.Infrastructure.Data;

public class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection")!;

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}