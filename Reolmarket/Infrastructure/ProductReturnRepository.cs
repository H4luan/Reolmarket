using System.Data;
using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class ProductReturnRepository
{
    public List<ReturnableSaleLine> GetReturnableSaleLines()
    {
        var lines = new List<ReturnableSaleLine>();

        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                sl.SaleLineID,
                s.SaleID,
                p.ProductID,
                p.Name,
                s.SaleDate,
                sl.Quantity,
                COALESCE(SUM(pr.Quantity), 0) AS ReturnedQuantity,
                sl.SalePrice
            FROM SaleLine AS sl
            INNER JOIN Sale AS s ON s.SaleID = sl.SaleID
            INNER JOIN Product AS p ON p.ProductID = sl.ProductID
            LEFT JOIN ProductReturn AS pr ON pr.SaleLineID = sl.SaleLineID
            GROUP BY
                sl.SaleLineID, s.SaleID, p.ProductID, p.Name, s.SaleDate,
                sl.Quantity, sl.SalePrice
            HAVING sl.Quantity > COALESCE(SUM(pr.Quantity), 0)
            ORDER BY s.SaleDate DESC, p.Name;
            """;

        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            lines.Add(new ReturnableSaleLine
            {
                SaleLineId = reader.GetInt32(0),
                SaleId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3),
                SaleDate = reader.GetDateTime(4),
                SoldQuantity = reader.GetInt32(5),
                ReturnedQuantity = reader.GetInt32(6),
                SalePrice = reader.GetDecimal(7)
            });
        }

        return lines;
    }

    public void AddReturn(int saleLineId, int quantity, string? comment)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity), "Returantal skal være større end 0.");
        }

        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();
        using SqlTransaction transaction =
            connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            int soldQuantity;
            int alreadyReturned;
            int productId;
            decimal salePrice;

            using (SqlCommand lookup = connection.CreateCommand())
            {
                lookup.Transaction = transaction;
                lookup.CommandText = """
                    SELECT
                        sl.Quantity,
                        sl.SalePrice,
                        sl.ProductID,
                        COALESCE(SUM(pr.Quantity), 0)
                    FROM SaleLine AS sl WITH (UPDLOCK, HOLDLOCK)
                    LEFT JOIN ProductReturn AS pr WITH (UPDLOCK, HOLDLOCK)
                        ON pr.SaleLineID = sl.SaleLineID
                    WHERE sl.SaleLineID = @SaleLineID
                    GROUP BY sl.Quantity, sl.SalePrice, sl.ProductID;
                    """;
                lookup.Parameters.AddWithValue("@SaleLineID", saleLineId);

                using SqlDataReader reader = lookup.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException(
                        "Salgslinjen blev ikke fundet.");
                }

                soldQuantity = reader.GetInt32(0);
                salePrice = reader.GetDecimal(1);
                productId = reader.GetInt32(2);
                alreadyReturned = reader.GetInt32(3);
            }

            if (quantity > soldQuantity - alreadyReturned)
            {
                throw new InvalidOperationException(
                    "Returantal må ikke overstige det resterende antal fra salget.");
            }

            using (SqlCommand insert = connection.CreateCommand())
            {
                insert.Transaction = transaction;
                insert.CommandText = """
                    INSERT INTO ProductReturn
                        (ReturnDate, Quantity, Amount, Comment, SaleLineID)
                    VALUES
                        (@ReturnDate, @Quantity, @Amount, @Comment, @SaleLineID);
                    """;
                insert.Parameters.AddWithValue("@ReturnDate", DateTime.Today);
                insert.Parameters.AddWithValue("@Quantity", quantity);
                insert.Parameters.AddWithValue(
                    "@Amount", salePrice * quantity);
                insert.Parameters.AddWithValue(
                    "@Comment", (object?)comment ?? DBNull.Value);
                insert.Parameters.AddWithValue("@SaleLineID", saleLineId);
                insert.ExecuteNonQuery();
            }

            using (SqlCommand stock = connection.CreateCommand())
            {
                stock.Transaction = transaction;
                stock.CommandText = """
                    UPDATE Product
                    SET StockQuantity = StockQuantity + @Quantity
                    WHERE ProductID = @ProductID;
                    """;
                stock.Parameters.AddWithValue("@Quantity", quantity);
                stock.Parameters.AddWithValue("@ProductID", productId);
                if (stock.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException(
                        "Produktet til salgslinjen blev ikke fundet.");
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}