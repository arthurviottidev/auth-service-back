using Dapper;

namespace AuthService.Infrastructure.Data;

public static class DapperConfig
{
    public static void Configure()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}