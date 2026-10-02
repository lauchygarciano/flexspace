using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL.Repositories;

public class ClienteRepository
{
    private readonly string _cs;
    public ClienteRepository(string connectionString) => _cs = connectionString;

    public Cliente? ObtenerPorId(int id)
    {
        using var cn = new SqliteConnection(_cs);
        cn.Open();
        using var cmd = new SqliteCommand(
            "SELECT Id, Nombre, Email, TipoCliente, SancionesActivas FROM Cliente WHERE Id = @id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public List<Cliente> ListarConSanciones()
    {
        var lista = new List<Cliente>();
        using var cn = new SqliteConnection(_cs);
        cn.Open();
        using var cmd = new SqliteCommand(
            "SELECT Id, Nombre, Email, TipoCliente, SancionesActivas FROM Cliente " +
            "WHERE SancionesActivas > 0 ORDER BY SancionesActivas DESC, Nombre", cn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) lista.Add(Mapear(r));
        return lista;
    }

    private static Cliente Mapear(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Nombre = r.GetString(1),
        Email = r.GetString(2),
        TipoCliente = Enum.Parse<TipoCliente>(r.GetString(3)),
        SancionesActivas = r.GetInt32(4)
    };
}
