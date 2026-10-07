using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class EmployeeRepository
{
    public List<Employee> GetAll()
    {
        var employees = new List<Employee>();

        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT EmployeeID, Name, Phone, Email
            FROM Employee
            ORDER BY Name;
            """;

        using SqlDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            employees.Add(new Employee
            {
                EmployeeId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Phone = reader.GetString(2),
                Email = reader.GetString(3)
            });
        }

        return employees;
    }

    public void Add(Employee employee)
    {
        using SqlConnection connection =
            DatabaseConnection.CreateConnection();

        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText =
            """
        INSERT INTO Employee (Name, Phone, Email)
        OUTPUT INSERTED.EmployeeID
        VALUES (@Name, @Phone, @Email);
        """;

        command.Parameters.AddWithValue("@Name", employee.Name);
        command.Parameters.AddWithValue("@Phone", employee.Phone);
        command.Parameters.AddWithValue("@Email", employee.Email);

        employee.EmployeeId =
            Convert.ToInt32(command.ExecuteScalar());
    }
}