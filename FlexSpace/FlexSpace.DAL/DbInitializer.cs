using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL;

/// <summary>Crea el esquema SQLite y carga datos de prueba la primera vez.</summary>
public static class DbInitializer
{
    public static void Inicializar(string connectionString)
    {
        using var cn = new SqliteConnection(connectionString);
        cn.Open();

        const string esquema = @"
CREATE TABLE IF NOT EXISTS Cliente (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Email TEXT NOT NULL,
    TipoCliente TEXT NOT NULL,
    SancionesActivas INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Puesto (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    TipoPuesto TEXT NOT NULL,
    TarifaBasePorHora REAL NOT NULL
);
CREATE TABLE IF NOT EXISTS Reserva (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER NOT NULL REFERENCES Cliente(Id),
    PuestoId INTEGER NOT NULL REFERENCES Puesto(Id),
    FechaInicio TEXT NOT NULL,
    FechaFin TEXT NOT NULL,
    Estado TEXT NOT NULL,
    CostoTotal REAL NOT NULL
);";
        using (var cmd = new SqliteCommand(esquema, cn)) cmd.ExecuteNonQuery();

        using var count = new SqliteCommand("SELECT COUNT(*) FROM Cliente", cn);
        if (Convert.ToInt32(count.ExecuteScalar()) > 0) return;

        const string seed = @"
INSERT INTO Cliente (Nombre, Email, TipoCliente, SancionesActivas) VALUES
 ('Ana Perez',   'ana@mail.com',   'Estandar', 0),
 ('Beto Gomez',  'beto@mail.com',  'VIP',      0),
 ('Carla Ruiz',  'carla@mail.com', 'Estandar', 1),
 ('Diego Lopez', 'diego@mail.com', 'Estandar', 3);
INSERT INTO Puesto (Codigo, TipoPuesto, TarifaBasePorHora) VALUES
 ('ESC-01',  'EscritorioIndividual', 1000),
 ('SALA-01', 'SalaReuniones',        5000),
 ('CAB-01',  'CabinaPrivada',        3000);";
        using var cmdSeed = new SqliteCommand(seed, cn);
        cmdSeed.ExecuteNonQuery();
    }
}
