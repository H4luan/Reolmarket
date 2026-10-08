using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class TenantRepository
{
    public List<Tenant> GetAll()
    {
        var tenants = new List<Tenant>();

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT ShelfRenterID, Name, Phone, Email,
                   UseCustomRent, CustomRentPerShelf
            FROM ShelfRenter
            ORDER BY Name;
            """;

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            tenants.Add(new Tenant
            {
                TenantId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.IsDBNull(2)
                    ? string.Empty
                    : reader.GetString(2),
                Email = reader.IsDBNull(3)
                    ? string.Empty
                    : reader.GetString(3),
                UseCustomRent = reader.GetBoolean(4),
                CustomRentPerShelf = reader.IsDBNull(5)
                    ? 0m
                    : reader.GetDecimal(5)
            });
        }

        return tenants;
    }

    public void Add(Tenant tenant)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO ShelfRenter (Name, Phone, Email)
            OUTPUT INSERTED.ShelfRenterID
            VALUES (@Name, @Phone, @Email);
            """;

        command.Parameters.AddWithValue("@Name", tenant.Name);
        command.Parameters.AddWithValue(
            "@Phone",
            (object?)tenant.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "@Email",
            (object?)tenant.Email ?? DBNull.Value);

        tenant.TenantId = (int)command.ExecuteScalar()!;
    }

    public void Update(Tenant tenant)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE ShelfRenter
            SET Name = @Name,
                Phone = @Phone,
                Email = @Email
            WHERE ShelfRenterID = @TenantID;
            """;

        command.Parameters.AddWithValue("@TenantID", tenant.TenantId);
        command.Parameters.AddWithValue("@Name", tenant.Name);
        command.Parameters.AddWithValue(
            "@Phone",
            (object?)tenant.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "@Email",
            (object?)tenant.Email ?? DBNull.Value);

        int rowsUpdated = command.ExecuteNonQuery();

        if (rowsUpdated == 0)
        {
            throw new InvalidOperationException(
                $"Lejer med ID {tenant.TenantId} blev ikke fundet.");
        }
    }

    public void UpdateRentPricing(Tenant tenant, DateTime effectiveDate)
    {
        if (tenant.UseCustomRent && tenant.CustomRentPerShelf <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tenant.CustomRentPerShelf),
                "Særprisen skal være større end 0 kr.");
        }

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using (SqlCommand tenantCommand = connection.CreateCommand())
            {
                tenantCommand.Transaction = transaction;
                tenantCommand.CommandText = """
                    UPDATE ShelfRenter
                    SET UseCustomRent = @UseCustomRent,
                        CustomRentPerShelf = @CustomRentPerShelf
                    WHERE ShelfRenterID = @TenantID;
                    """;
                tenantCommand.Parameters.AddWithValue("@TenantID", tenant.TenantId);
                tenantCommand.Parameters.AddWithValue("@UseCustomRent", tenant.UseCustomRent);
                tenantCommand.Parameters.AddWithValue(
                    "@CustomRentPerShelf",
                    tenant.UseCustomRent
                        ? tenant.CustomRentPerShelf
                        : DBNull.Value);

                if (tenantCommand.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        $"Lejer med ID {tenant.TenantId} blev ikke fundet.");
                }
            }

            var agreementsToReprice = new List<(int RentalId, DateTime StartDate)>();
            using (SqlCommand countCommand = connection.CreateCommand())
            {
                countCommand.Transaction = transaction;
                countCommand.CommandText = """
                    SELECT RentalAgreementID, StartDate
                    FROM RentalAgreement
                    WHERE ShelfRenterID = @TenantID
                      AND (EndDate IS NULL OR EndDate >= @EffectiveDate);
                    """;
                countCommand.Parameters.AddWithValue("@TenantID", tenant.TenantId);
                countCommand.Parameters.AddWithValue("@EffectiveDate", effectiveDate.Date);
                using SqlDataReader reader = countCommand.ExecuteReader();
                while (reader.Read())
                {
                    agreementsToReprice.Add((
                        reader.GetInt32(0),
                        reader.GetDateTime(1).Date));
                }
            }

            var shelfCountsByDate = new Dictionary<DateTime, int>();
            foreach ((int rentalId, DateTime startDate) in agreementsToReprice)
            {
                DateTime pricingDate = startDate > effectiveDate.Date
                    ? startDate
                    : effectiveDate.Date;

                decimal price;
                if (tenant.UseCustomRent)
                {
                    price = tenant.CustomRentPerShelf;
                }
                else
                {
                    if (!shelfCountsByDate.TryGetValue(pricingDate, out int shelfCount))
                    {
                        using SqlCommand countShelvesCommand = connection.CreateCommand();
                        countShelvesCommand.Transaction = transaction;
                        countShelvesCommand.CommandText = """
                            SELECT COUNT(*)
                            FROM RentalAgreement
                            WHERE ShelfRenterID = @TenantID
                              AND StartDate <= @PricingDate
                              AND (EndDate IS NULL OR EndDate >= @PricingDate);
                            """;
                        countShelvesCommand.Parameters.AddWithValue("@TenantID", tenant.TenantId);
                        countShelvesCommand.Parameters.AddWithValue("@PricingDate", pricingDate);
                        shelfCount = Convert.ToInt32(countShelvesCommand.ExecuteScalar());
                        shelfCountsByDate[pricingDate] = shelfCount;
                    }

                    price = shelfCount switch
                    {
                        1 => 850m,
                        <= 3 => 825m,
                        _ => 800m
                    };
                }

                using SqlCommand rentalCommand = connection.CreateCommand();
                rentalCommand.Transaction = transaction;
                rentalCommand.CommandText = """
                    UPDATE RentalAgreement
                    SET Price = @Price
                    WHERE RentalAgreementID = @RentalID;
                    """;
                rentalCommand.Parameters.AddWithValue("@RentalID", rentalId);
                rentalCommand.Parameters.AddWithValue("@Price", price);
                rentalCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Delete(int tenantId)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            using SqlCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                DELETE FROM Settlement
                WHERE ShelfRenterID = @TenantID;

                DELETE FROM RentalAgreement
                WHERE ShelfRenterID = @TenantID;

                DELETE FROM ShelfRenter
                WHERE ShelfRenterID = @TenantID;

                IF @@ROWCOUNT = 0
                    THROW 50002, 'Lejeren blev ikke fundet.', 1;
                """;
            command.Parameters.AddWithValue("@TenantID", tenantId);
            command.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
