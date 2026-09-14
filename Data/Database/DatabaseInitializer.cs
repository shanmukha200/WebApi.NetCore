using System.Text.RegularExpressions;

namespace WebApi.NetCore.Data.Database;

public class DatabaseInitializer(IDataAccess dataAccess, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Database", "schema.sql");

        if (!File.Exists(scriptPath))
        {
            logger.LogWarning("Database schema script not found at path: {ScriptPath}", scriptPath);
            return;
        }

        var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        var commands = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(command => !string.IsNullOrWhiteSpace(command));

        foreach (var command in commands)
        {
            await dataAccess.ExecuteAsync(command, cancellationToken: cancellationToken);
        }

        logger.LogInformation("Database schema initialization completed.");
    }
}
