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
                    SELECT COUNT(*) * CASE WHEN COUNT(*) = 1 THEN 850.00 WHEN COUNT(*) <= 3 THEN 825.00 ELSE 800.00 END
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