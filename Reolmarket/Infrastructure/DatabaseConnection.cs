using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Reolmarket.Infrastructure
{
    public static class  DatabaseConnection
    {
        private const string ConnectionString =
             "Server=localhost;Database=ShelfMarket;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection()


        { 
           return new SqlConnection(ConnectionString);
        }

    }
    
    
}
