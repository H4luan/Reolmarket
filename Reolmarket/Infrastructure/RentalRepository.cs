using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class RentalRepository
{
    public List<Rental> GetAll()
    {
        var rentals = new List<Rental>();

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                ra.RentalAgreementID,
                ra.ShelfRenterID,
                sr.Name,
                ra.ShelfID,
                s.Number,
                ra.StartDate,
                ra.EndDate,
                ra.Price,
                ra.PaymentMethod
            FROM RentalAgreement AS ra
            INNER JOIN ShelfRenter AS sr
                ON sr.ShelfRenterID = ra.ShelfRenterID
            INNER JOIN Shelf AS s
                ON s.ShelfID = ra.ShelfID
            ORDER BY ra.StartDate DESC;
            """;

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            rentals.Add(new Rental
            {
                RentalId = reader.GetInt32(0),
                TenantId = reader.GetInt32(1),
                Tenant = new Tenant
                {
                    TenantId = reader.GetInt32(1),
                    Name = reader.GetString(2)
                },
                ShelfId = reader.GetInt32(3),
                Shelf = new Shelf
                {
                    ShelfId = reader.GetInt32(3),
                    ShelfNumber = reader.IsDBNull(4)
                        ? 0
                        : reader.GetInt32(4)
                },
                StartDate = reader.GetDateTime(5),
                EndDate = reader.IsDBNull(6)
                    ? null
                    : reader.GetDateTime(6),
                MonthlyRent = reader.GetDecimal(7),
                PaymentMethod = Enum.Parse<PaymentMethod>(reader.GetString(8))
            });
        }

        return rentals;
    }

    public void Add(Rental rental)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO RentalAgreement
                (StartDate, EndDate, Price, ShelfRenterID, ShelfID, PaymentMethod)
            OUTPUT INSERTED.RentalAgreementID
            VALUES
                (@StartDate, @EndDate, @Price, @ShelfRenterID, @ShelfID, @PaymentMethod);
            """;

        command.Parameters.AddWithValue("@StartDate", rental.StartDate);
        command.Parameters.AddWithValue(
            "@EndDate",
            (object?)rental.EndDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", rental.MonthlyRent);
        command.Parameters.AddWithValue("@ShelfRenterID", rental.TenantId);
        command.Parameters.AddWithValue("@ShelfID", rental.ShelfId);

        command.Parameters.AddWithValue("@PaymentMethod", rental.PaymentMethod.ToString());

        rental.RentalId = (int)command.ExecuteScalar()!;
    }


    public void AddAndRepriceActiveRentals(Rental rental)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using SqlCommand updateCommand = connection.CreateCommand();
            updateCommand.Transaction = transaction;
            updateCommand.CommandText =
                """
            UPDATE RentalAgreement
            SET Price = @Price
            WHERE ShelfRenterID = @ShelfRenterID
              AND StartDate <= @StartDate
              AND (EndDate IS NULL OR EndDate >= @StartDate);
            """;

            updateCommand.Parameters.AddWithValue("@Price", rental.MonthlyRent);
            updateCommand.Parameters.AddWithValue(
                "@ShelfRenterID",
                rental.TenantId);
            updateCommand.Parameters.AddWithValue(
                "@StartDate",
                rental.StartDate.Date);

            updateCommand.ExecuteNonQuery();

            using SqlCommand insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText =
                """
            INSERT INTO RentalAgreement
                (StartDate, EndDate, Price, ShelfRenterID, ShelfID, PaymentMethod)
            OUTPUT INSERTED.RentalAgreementID
            VALUES
                (@StartDate, @EndDate, @Price, @ShelfRenterID, @ShelfID, @PaymentMethod);
            """;

            insertCommand.Parameters.AddWithValue(
                "@StartDate",
                rental.StartDate.Date);
            insertCommand.Parameters.AddWithValue(
                "@EndDate",
                (object?)rental.EndDate?.Date ?? DBNull.Value);
            insertCommand.Parameters.AddWithValue(
                "@Price",
                rental.MonthlyRent);
            insertCommand.Parameters.AddWithValue(
                "@ShelfRenterID",
                rental.TenantId);
            insertCommand.Parameters.AddWithValue(
                "@ShelfID",
                rental.ShelfId);


            insertCommand.Parameters.AddWithValue("@PaymentMethod", rental.PaymentMethod.ToString());
            rental.RentalId =
                Convert.ToInt32(insertCommand.ExecuteScalar());

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool IsShelfAvailable(
    int shelfId,
    DateTime startDate,
    DateTime? endDate)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
        SELECT COUNT(*)
        FROM RentalAgreement
        WHERE ShelfID = @ShelfID
          AND StartDate <= COALESCE(
                @EndDate,
                CONVERT(date, '99991231'))
          AND (EndDate IS NULL OR EndDate >= @StartDate);
        """;

        command.Parameters.AddWithValue("@ShelfID", shelfId);
        command.Parameters.AddWithValue("@StartDate", startDate.Date);
        command.Parameters.AddWithValue(
            "@EndDate",
            (object?)endDate?.Date ?? DBNull.Value);

        int overlappingRentals = (int)command.ExecuteScalar()!;

        return overlappingRentals == 0;
    }


    public void Update(Rental rental)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE RentalAgreement
            SET StartDate = @StartDate,
                EndDate = @EndDate,
                Price = @Price,
                ShelfRenterID = @ShelfRenterID,
                ShelfID = @ShelfID,
                PaymentMethod = @PaymentMethod
            WHERE RentalAgreementID = @RentalAgreementID;
            """;

        command.Parameters.AddWithValue(
            "@RentalAgreementID",
            rental.RentalId);
        command.Parameters.AddWithValue("@StartDate", rental.StartDate);
        command.Parameters.AddWithValue(
            "@EndDate",
            (object?)rental.EndDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", rental.MonthlyRent);
        command.Parameters.AddWithValue("@ShelfRenterID", rental.TenantId);
        command.Parameters.AddWithValue("@ShelfID", rental.ShelfId);

        command.Parameters.AddWithValue("@PaymentMethod", rental.PaymentMethod.ToString());

        int rowsUpdated = command.ExecuteNonQuery();

        if (rowsUpdated == 0)
        {
            throw new InvalidOperationException(
                $"Lejeaftale med ID {rental.RentalId} blev ikke fundet.");
        }
    }

    public void Delete(int rentalId)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM RentalAgreement
            WHERE RentalAgreementID = @RentalAgreementID;
            """;

        command.Parameters.AddWithValue(
            "@RentalAgreementID",
            rentalId);

        int rowsDeleted = command.ExecuteNonQuery();

        if (rowsDeleted == 0)
        {
            throw new InvalidOperationException(
                $"Lejeaftale med ID {rentalId} blev ikke fundet.");
        }
    }
}