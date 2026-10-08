using Microsoft.Data.SqlClient;
using Reolmarket.Domain;

namespace Reolmarket.Infrastructure;

public class SettlementRepository
{
    public Settlement Calculate(int tenantId, DateTime selectedMonth)
    {
        DateTime periodStart = new(selectedMonth.Year, selectedMonth.Month, 1);
        DateTime nextMonthStart = periodStart.AddMonths(1);
        DateTime followingMonthStart = periodStart.AddMonths(2);
        DateTime periodEnd = nextMonthStart.AddDays(-1);

        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                COALESCE((
                    SELECT SUM(sl.Quantity * sl.SalePrice)
                    FROM Sale AS s
                    INNER JOIN SaleLine AS sl ON sl.SaleID = s.SaleID
                    INNER JOIN RentalAgreement AS ra
                        ON ra.ShelfID = sl.ShelfID
                       AND ra.ShelfRenterID = @TenantID
                       AND ra.StartDate <= s.SaleDate
                       AND (ra.EndDate IS NULL OR ra.EndDate >= s.SaleDate)
                    WHERE s.SaleDate >= @PeriodStart
                      AND s.SaleDate < @NextMonthStart
                ), 0) AS GrossSales,
                COALESCE((
                    SELECT SUM(pr.Amount)
                    FROM ProductReturn AS pr
                    INNER JOIN SaleLine AS sl ON sl.SaleLineID = pr.SaleLineID
                    INNER JOIN Sale AS s ON s.SaleID = sl.SaleID
                    INNER JOIN RentalAgreement AS ra
                        ON ra.ShelfID = sl.ShelfID
                       AND ra.ShelfRenterID = @TenantID
                       AND ra.StartDate <= s.SaleDate
                       AND (ra.EndDate IS NULL OR ra.EndDate >= s.SaleDate)
                    WHERE pr.ReturnDate >= @PeriodStart
                      AND pr.ReturnDate < @NextMonthStart
                ), 0) AS TotalReturns,
                COALESCE((
                    SELECT SUM(ra.Price)
                    FROM RentalAgreement AS ra
                    WHERE ra.ShelfRenterID = @TenantID
                      AND ra.StartDate < @FollowingMonthStart
                      AND (ra.EndDate IS NULL OR ra.EndDate >= @NextMonthStart)
                ), 0) AS RentForNextPeriod;
            """;
        command.Parameters.AddWithValue("@TenantID", tenantId);
        command.Parameters.AddWithValue("@PeriodStart", periodStart);
        command.Parameters.AddWithValue("@NextMonthStart", nextMonthStart);
        command.Parameters.AddWithValue("@FollowingMonthStart", followingMonthStart);

        using SqlDataReader reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException(
                "Afregningen kunne ikke beregnes.");
        }

        var settlement = new Settlement
        {
            TenantId = tenantId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GrossSales = reader.GetDecimal(0),
            TotalReturns = reader.GetDecimal(1),
            RentForNextPeriod = reader.GetDecimal(2)
        };
        settlement.CalculateCommission();
        return settlement;
    }

    public List<Settlement> GetForTenant(int tenantId)
    {
        var settlements = new List<Settlement>();

        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.SettlementID, s.SettlementDate, s.TotalSales, s.TotalReturns,
                   s.Commission, s.TotalRent, s.LastModifiedAt,
                   s.ModifiedByEmployeeID, COALESCE(e.Name, '')
            FROM Settlement AS s
            LEFT JOIN Employee AS e ON e.EmployeeID = s.ModifiedByEmployeeID
            WHERE s.ShelfRenterID = @TenantID
            ORDER BY s.SettlementDate DESC, s.SettlementID DESC;
            """;
        command.Parameters.AddWithValue("@TenantID", tenantId);

        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            DateTime periodEnd = reader.GetDateTime(1);
            settlements.Add(new Settlement
            {
                SettlementId = reader.GetInt32(0),
                TenantId = tenantId,
                PeriodStart = new DateTime(periodEnd.Year, periodEnd.Month, 1),
                PeriodEnd = periodEnd,
                GrossSales = reader.GetDecimal(2),
                TotalReturns = reader.GetDecimal(3),
                Commission = reader.GetDecimal(4),
                RentForNextPeriod = reader.GetDecimal(5),
                IsFinalized = true,
                LastModifiedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ModifiedByEmployeeId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                ModifiedByEmployeeName = reader.GetString(8)
            });
        }

        return settlements;
    }
    public void Update(int settlementId, Settlement settlement, int employeeId)
    {
        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Settlement
            SET SettlementDate = @SettlementDate,
                TotalSales = @TotalSales,
                TotalReturns = @TotalReturns,
                Commission = @Commission,
                TotalRent = @TotalRent,
                Result = @Result,
                LastModifiedAt = SYSDATETIME(),
                ModifiedByEmployeeID = @EmployeeID
            WHERE SettlementID = @SettlementID
              AND ShelfRenterID = @TenantID;
            """;
        command.Parameters.AddWithValue("@SettlementID", settlementId);
        command.Parameters.AddWithValue("@TenantID", settlement.TenantId);
        command.Parameters.AddWithValue("@EmployeeID", employeeId);
        command.Parameters.AddWithValue("@SettlementDate", settlement.PeriodEnd.Date);
        command.Parameters.AddWithValue("@TotalSales", settlement.GrossSales);
        command.Parameters.AddWithValue("@TotalReturns", settlement.TotalReturns);
        command.Parameters.AddWithValue("@Commission", settlement.Commission);
        command.Parameters.AddWithValue("@TotalRent", settlement.RentForNextPeriod);
        command.Parameters.AddWithValue("@Result", settlement.NetAmount);

        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException("Den gemte afregning blev ikke fundet.");
        }
    }
    public int Save(Settlement settlement)
    {
        using SqlConnection connection = DatabaseConnection.CreateConnection();
        connection.Open();

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = """
            IF EXISTS (
                SELECT 1
                FROM Settlement
                WHERE ShelfRenterID = @TenantID
                  AND SettlementDate = @SettlementDate
            )
            BEGIN
                THROW 50001, 'Der findes allerede en afregning for lejeren og måneden.', 1;
            END;

            INSERT INTO Settlement
                (SettlementDate, TotalSales, TotalReturns, Commission, TotalRent, Result, ShelfRenterID)
            OUTPUT INSERTED.SettlementID
            VALUES
                (@SettlementDate, @TotalSales, @TotalReturns, @Commission, @TotalRent, @Result, @TenantID);
            """;
        command.Parameters.AddWithValue("@TenantID", settlement.TenantId);
        command.Parameters.AddWithValue("@SettlementDate", settlement.PeriodEnd.Date);
        command.Parameters.AddWithValue("@TotalSales", settlement.GrossSales);
        command.Parameters.AddWithValue("@TotalReturns", settlement.TotalReturns);
        command.Parameters.AddWithValue("@Commission", settlement.Commission);
        command.Parameters.AddWithValue("@TotalRent", settlement.RentForNextPeriod);
        command.Parameters.AddWithValue("@Result", settlement.NetAmount);

        return Convert.ToInt32(command.ExecuteScalar());
    }
}

