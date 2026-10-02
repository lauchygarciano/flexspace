using System.Globalization;
using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL.Repositories;

public class ReservaRepository
{
    private const string Formato = "yyyy-MM-dd HH:mm:ss";
    private readonly string _cs;
    public ReservaRepository(string connectionString) => _cs = connectionString;

    /// <summary>Hay solapamiento si inicioExistente &lt; finNuevo Y finExistente &gt; inicioNuevo.</summary>
    public bool ExisteSolapamiento(int puestoId, DateTime inicio, DateTime fin)
    {
        using var cn = new SqliteConnection(_cs);
        cn.Open();
        using var cmd = new SqliteCommand(
            "SELECT COUNT(*) FROM Reserva WHERE PuestoId = @p AND Estado = 'Confirmada' " +
            "AND FechaInicio < @fin AND FechaFin > @ini", cn);
        cmd.Parameters.AddWithValue("@p", puestoId);
        cmd.Parameters.AddWithValue("@ini", inicio.ToString(Formato, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@fin", fin.ToString(Formato, CultureInfo.InvariantCulture));
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insertar(Reserva reserva)
    {
        using var cn = new SqliteConnection(_cs);
        cn.Open();
        using var cmd = new SqliteCommand(
            "INSERT INTO Reserva (ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal) " +
            "VALUES (@c, @p, @ini, @fin, @e, @costo); SELECT last_insert_rowid();", cn);
        cmd.Parameters.AddWithValue("@c", reserva.ClienteId);
        cmd.Parameters.AddWithValue("@p", reserva.PuestoId);
        cmd.Parameters.AddWithValue("@ini", reserva.FechaInicio.ToString(Formato, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@fin", reserva.FechaFin.ToString(Formato, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("@e", reserva.Estado.ToString());
        cmd.Parameters.AddWithValue("@costo", (double)reserva.CostoTotal);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
