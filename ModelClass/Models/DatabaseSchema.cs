using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace ModelClass.Models;

public static partial class DatabaseSchema
{
    public const string ConfigurationKey = "Database:Schema";
    public const string DefaultName = "geovise";

    public static string Get(IConfiguration configuration)
    {
        return Validate(configuration[ConfigurationKey] ?? DefaultName);
    }

    public static string GetForMigrations() =>
        Validate(Environment.GetEnvironmentVariable("Database__Schema") ?? DefaultName);

    public static string NextValue(string schema, string sequenceName) =>
        $"nextval('\"{schema}\".\"{sequenceName}\"'::regclass)";

    private static string Validate(string schema)
    {
        if (string.IsNullOrWhiteSpace(schema))
        {
            throw new InvalidOperationException($"Configuration value '{ConfigurationKey}' cannot be empty.");
        }

        if (!SchemaNamePattern().IsMatch(schema))
        {
            throw new InvalidOperationException(
                $"Configuration value '{ConfigurationKey}' must be a PostgreSQL identifier.");
        }

        return schema;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex SchemaNamePattern();
}
