using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class SaleRepository
{
    public void Add(Sale sale)
    {
        if (sale.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Et salg skal indeholde mindst ét produkt.");
        }

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using SqlCommand saleCommand = connection.CreateCommand();
            saleCommand.Transaction = transaction;
            saleCommand.CommandText =
                """
                INSERT INTO Sale (SaleDate, TotalAmount, EmployeeID, PaymentMethod)
                OUTPUT INSERTED.SaleID
                VALUES (@SaleDate, @TotalAmount, @EmployeeID, @PaymentMethod);
                """;

            saleCommand.Parameters.AddWithValue(
                "@SaleDate",
                sale.SoldAt.Date);
            saleCommand.Parameters.AddWithValue(
                "@TotalAmount",
                sale.TotalAmount);
            saleCommand.Parameters.AddWithValue(
                "@EmployeeID",
                sale.EmployeeId);
            saleCommand.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod.ToString());

            sale.SaleId =
                Convert.ToInt32(saleCommand.ExecuteScalar());

            foreach (SaleLine line in sale.Lines)
            {
                using SqlCommand stockCommand = connection.CreateCommand();
                stockCommand.Transaction = transaction;
                stockCommand.CommandText =
                    """
                    UPDATE Product
                    SET StockQuantity = StockQuantity - @Quantity
                    WHERE ProductID = @ProductID
                      AND StockQuantity >= @Quantity;
                    """;

                stockCommand.Parameters.AddWithValue(
                    "@Quantity",
                    line.Quantity);
                stockCommand.Parameters.AddWithValue(
                    "@ProductID",
                    line.ProductId);

                int rowsUpdated = stockCommand.ExecuteNonQuery();

                if (rowsUpdated == 0)
                {
                    throw new InvalidOperationException(
                        $"Produkt med ID {line.ProductId} findes ikke, " +
                        "eller der er ikke nok på lager.");
                }

                using SqlCommand lineCommand = connection.CreateCommand();
                lineCommand.Transaction = transaction;
                lineCommand.CommandText =
                    """
                    INSERT INTO SaleLine
                        (Quantity, SalePrice, ProductID, SaleID, ShelfID, Comment)
                    OUTPUT INSERTED.SaleLineID
                    VALUES
                        (@Quantity, @SalePrice, @ProductID, @SaleID, @ShelfID, @Comment);
                    """;

                lineCommand.Parameters.AddWithValue(
                    "@Quantity",
                    line.Quantity);
                lineCommand.Parameters.AddWithValue(
                    "@SalePrice",
                    line.SalePrice);
                lineCommand.Parameters.AddWithValue(
                    "@ProductID",
                    line.ProductId);
                lineCommand.Parameters.AddWithValue(
                    "@SaleID",
                    sale.SaleId);
                lineCommand.Parameters.AddWithValue("@ShelfID", line.ShelfId);
                lineCommand.Parameters.AddWithValue("@Comment", (object?)line.Comment ?? DBNull.Value);

                line.SaleId = sale.SaleId;
                line.SaleLineId =
                    Convert.ToInt32(lineCommand.ExecuteScalar());
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
