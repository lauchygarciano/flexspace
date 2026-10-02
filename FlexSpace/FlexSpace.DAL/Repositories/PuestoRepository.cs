using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL.Repositories;

public class PuestoRepository
{
    private readonly string _cs;
    public PuestoRepository(string connectionString) => _cs = connectionString;

    public Puesto? ObtenerPorId(int id)
    {
        using var cn = new SqliteConnection(_cs);
        cn.Open();
        using var cmd = new SqliteCommand(
            "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puesto WHERE Id = @id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Puesto
        {
            Id = r.GetInt32(0),
            Codigo = r.GetString(1),
            TipoPuesto = Enum.Parse<TipoPuesto>(r.GetString(2)),
            TarifaBasePorHora = Convert.ToDecimal(r.GetDouble(3))
        };
    }
}
