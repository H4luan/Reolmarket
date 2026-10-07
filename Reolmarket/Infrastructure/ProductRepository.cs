using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class ProductRepository
{
    public List<Product> GetAll()
    {
        var products = new List<Product>();

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ProductID, Name, Price, Barcode, StockQuantity, ShelfID
            FROM Product
            ORDER BY Name;
            """;

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            products.Add(new Product
            {
                ProductId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Barcode = reader.GetString(3),
                StockQuantity = reader.GetInt32(4),
                ShelfId = reader.GetInt32(5)
            });
        }

        return products;
    }

    public void Add(Product product)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Product
                (Name, Price, Barcode, StockQuantity, ShelfID)
            OUTPUT INSERTED.ProductID
            VALUES
                (@Name, @Price, @Barcode, @StockQuantity, @ShelfID);
            """;

        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Barcode", product.Barcode);
        command.Parameters.AddWithValue(
            "@StockQuantity",
            product.StockQuantity);
        command.Parameters.AddWithValue("@ShelfID", product.ShelfId);

        product.ProductId = Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Product product)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Product
            SET Name = @Name,
                Price = @Price,
                Barcode = @Barcode,
                StockQuantity = @StockQuantity,
                ShelfID = @ShelfID
            WHERE ProductID = @ProductID;
            """;

        command.Parameters.AddWithValue("@ProductID", product.ProductId);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Barcode", product.Barcode);
        command.Parameters.AddWithValue(
            "@StockQuantity",
            product.StockQuantity);
        command.Parameters.AddWithValue("@ShelfID", product.ShelfId);

        int rowsUpdated = command.ExecuteNonQuery();

        if (rowsUpdated == 0)
        {
            throw new InvalidOperationException(
                $"Produkt med ID {product.ProductId} blev ikke fundet.");
        }
    }

    public void Delete(int productId)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM Product
            WHERE ProductID = @ProductID;
            """;

        command.Parameters.AddWithValue("@ProductID", productId);

        int rowsDeleted = command.ExecuteNonQuery();

        if (rowsDeleted == 0)
        {
            throw new InvalidOperationException(
                $"Produkt med ID {productId} blev ikke fundet.");
        }
    }
}