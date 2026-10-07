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
            SELECT ShelfRenterID, Name, Phone, Email
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
                    : reader.GetString(3)
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

    public void Delete(int tenantId)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM ShelfRenter
            WHERE ShelfRenterID = @TenantID;
            """;

        command.Parameters.AddWithValue("@TenantID", tenantId);

        int rowsDeleted = command.ExecuteNonQuery();

        if (rowsDeleted == 0)
        {
            throw new InvalidOperationException(
                $"Lejer med ID {tenantId} blev ikke fundet.");
        }
    }
}