using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Reolmarket.Infrastructure;

public static class DatabaseInitializer
{
    private const string SetupResourceName =
        "Reolmarket.Database.InitializeShelfMarket.sql";

    public static void Initialize()
    {
        using (SqlConnection connection = DatabaseConnection.CreateMasterConnection())
        {
            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = """
                IF DB_ID(N'ShelfMarket') IS NULL
                BEGIN
                    CREATE DATABASE [ShelfMarket];
                END;
                """;
            command.ExecuteNonQuery();
        }

        string setupScript = ReadSetupScript();
        using SqlConnection databaseConnection = DatabaseConnection.CreateConnection();
        databaseConnection.Open();

        string[] batches = Regex.Split(
            setupScript,
            @"^\s*GO\s*(?:--.*)?$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (string batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            using SqlCommand setupCommand = databaseConnection.CreateCommand();
            setupCommand.CommandText = batch;
            setupCommand.CommandTimeout = 60;
            setupCommand.ExecuteNonQuery();
        }
    }

    private static string ReadSetupScript()
    {
        using Stream stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(SetupResourceName)
            ?? throw new InvalidOperationException(
                "Databasens opsætningsscript blev ikke fundet i programmet.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
