using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Reolmarket.Infrastructure
{
    public static class  DatabaseConnection
    {
        private static readonly string ServerName =
            Environment.GetEnvironmentVariable("REOLMARKET_SQL_SERVER") ?? "localhost";
        private const string ConnectionOptions =
            "Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection() =>
            new($"Server={ServerName};Database=ShelfMarket;{ConnectionOptions}");

        public static SqlConnection CreateMasterConnection() =>
            new($"Server={ServerName};Database=master;{ConnectionOptions}");

    }
    
    
}
