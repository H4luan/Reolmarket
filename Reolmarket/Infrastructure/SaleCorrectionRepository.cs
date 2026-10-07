using Microsoft.Data.SqlClient;
using Reolmarket.Domain;
namespace Reolmarket.Infrastructure;
public class SaleCorrectionRepository
{
    public List<SaleLineCorrection> GetAll()
    {
        var rows = new List<SaleLineCorrection>();
        using var c = DatabaseConnection.CreateConnection(); c.Open();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT sl.SaleLineID,s.SaleID,s.SaleDate,p.Name,sl.Quantity,sl.SalePrice,sl.ShelfID,sh.Number,sl.Comment FROM SaleLine sl JOIN Sale s ON s.SaleID=sl.SaleID JOIN Product p ON p.ProductID=sl.ProductID JOIN Shelf sh ON sh.ShelfID=sl.ShelfID ORDER BY s.SaleDate DESC,s.SaleID DESC";
        using var r = q.ExecuteReader();
        while (r.Read()) rows.Add(new SaleLineCorrection { SaleLineId=r.GetInt32(0),SaleId=r.GetInt32(1),SaleDate=r.GetDateTime(2),ProductName=r.GetString(3),Quantity=r.GetInt32(4),SalePrice=r.GetDecimal(5),ShelfId=r.GetInt32(6),ShelfNumber=r.GetInt32(7),Comment=r.IsDBNull(8)?null:r.GetString(8) });
        return rows;
    }
    public void Update(SaleLineCorrection line)
    {
        if (line.SalePrice < 0 || line.ShelfId <= 0) throw new InvalidOperationException("Kontrollér pris og reol.");
        using var c=DatabaseConnection.CreateConnection(); c.Open(); using var tx=c.BeginTransaction();
        try
        {
            using var q=c.CreateCommand(); q.Transaction=tx;
            q.CommandText="UPDATE SaleLine SET SalePrice=@Price,ShelfID=@ShelfID,Comment=@Comment WHERE SaleLineID=@LineID; UPDATE ProductReturn SET Amount=@Price*Quantity WHERE SaleLineID=@LineID; UPDATE Sale SET TotalAmount=(SELECT SUM(Quantity*SalePrice) FROM SaleLine WHERE SaleID=@SaleID) WHERE SaleID=@SaleID;";
            q.Parameters.AddWithValue("@Price",line.SalePrice); q.Parameters.AddWithValue("@ShelfID",line.ShelfId); q.Parameters.AddWithValue("@Comment",(object?)line.Comment??DBNull.Value); q.Parameters.AddWithValue("@LineID",line.SaleLineId); q.Parameters.AddWithValue("@SaleID",line.SaleId); q.ExecuteNonQuery(); tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }
}
