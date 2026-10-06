using System.Globalization;
using Microsoft.Data.Sqlite;
using Entidades;

namespace Datos
{
    // se encarga de crear la base de datos y las tablas si no existen
    public static class Conexion
    {
        public const string CadenaConexion = "Data Source=flexspace.db";
        public const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";
        private static bool _inicializada = false;

        public static void Inicializar()
        {
            if (_inicializada) return;

            using (SqliteConnection conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();

                string crearCliente = @"
                CREATE TABLE IF NOT EXISTS Cliente (
                    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre           TEXT NOT NULL,
                    Email            TEXT NOT NULL,
                    TipoCliente      TEXT NOT NULL,
                    SancionesActivas INTEGER NOT NULL DEFAULT 0
                )";

                string crearPuesto = @"
                CREATE TABLE IF NOT EXISTS Puesto (
                    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo            TEXT NOT NULL UNIQUE,
                    TipoPuesto        TEXT NOT NULL,
                    TarifaBasePorHora REAL NOT NULL
                )";

                string crearReserva = @"
                CREATE TABLE IF NOT EXISTS Reserva (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClienteId   INTEGER NOT NULL,
                    PuestoId    INTEGER NOT NULL,
                    FechaInicio TEXT NOT NULL,
                    FechaFin    TEXT NOT NULL,
                    Estado      TEXT NOT NULL,
                    CostoTotal  REAL NOT NULL,
                    FOREIGN KEY (ClienteId) REFERENCES Cliente(Id),
                    FOREIGN KEY (PuestoId) REFERENCES Puesto(Id)
                )";

                foreach (string sql in new[] { crearCliente, crearPuesto, crearReserva })
                {
                    using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                    {
                        comando.ExecuteNonQuery();
                    }
                }

                //datos de prueba, solo si la tabla esta vacia
                using (SqliteCommand contar = new SqliteCommand("SELECT COUNT(*) FROM Cliente", conexion))
                {
                    if (Convert.ToInt32(contar.ExecuteScalar()) == 0)
                    {
                        string datosPrueba = @"
                        INSERT INTO Cliente (Nombre, Email, TipoCliente, SancionesActivas) VALUES
                            ('Ana Perez',   'ana@mail.com',   'Estandar', 0),
                            ('Beto Gomez',  'beto@mail.com',  'VIP',      0),
                            ('Carla Ruiz',  'carla@mail.com', 'Estandar', 1),
                            ('Diego Lopez', 'diego@mail.com', 'Estandar', 3);
                        INSERT INTO Puesto (Codigo, TipoPuesto, TarifaBasePorHora) VALUES
                            ('ESC-01',  'EscritorioIndividual', 1000),
                            ('SALA-01', 'SalaReuniones',        5000),
                            ('CAB-01',  'CabinaPrivada',        3000);";
                        using (SqliteCommand comando = new SqliteCommand(datosPrueba, conexion))
                        {
                            comando.ExecuteNonQuery();
                        }
                    }
                }
            }

            _inicializada = true;
        }
    }

    public class ClienteDatos
    {
        public ClienteDatos()
        {
            Conexion.Inicializar();
        }

        private const string ColumnasSelect = "Id, Nombre, Email, TipoCliente, SancionesActivas";

        private Cliente LeerFila(SqliteDataReader reader)
        {
            return new Cliente
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString() ?? "",
                Email = reader["Email"].ToString() ?? "",
                TipoCliente = Enum.Parse<TipoCliente>(reader["TipoCliente"].ToString() ?? ""),
                SancionesActivas = Convert.ToInt32(reader["SancionesActivas"])
            };
        }

        public Cliente? ObtenerPorId(int id)
        {
            string query = $"SELECT {ColumnasSelect} FROM Cliente WHERE Id = @Id";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                SqliteCommand comando = new SqliteCommand(query, conexion);
                comando.Parameters.AddWithValue("@Id", id);

                conexion.Open();

                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read()) return LeerFila(reader);
                }
            }

            return null;
        }

        public List<Cliente> ListarSancionados()
        {
            var resultados = new List<Cliente>();

            string query = $"SELECT {ColumnasSelect} FROM Cliente WHERE SancionesActivas > 0 ORDER BY SancionesActivas DESC";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                SqliteCommand comando = new SqliteCommand(query, conexion);

                conexion.Open();

                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultados.Add(LeerFila(reader));
                    }
                }
            }

            return resultados;
        }
    }

    public class PuestoDatos
    {
        public PuestoDatos()
        {
            Conexion.Inicializar();
        }

        public Puesto? ObtenerPorId(int id)
        {
            string query = "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puesto WHERE Id = @Id";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                SqliteCommand comando = new SqliteCommand(query, conexion);
                comando.Parameters.AddWithValue("@Id", id);

                conexion.Open();

                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Puesto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Codigo = reader["Codigo"].ToString() ?? "",
                            TipoPuesto = Enum.Parse<TipoPuesto>(reader["TipoPuesto"].ToString() ?? ""),
                            TarifaBasePorHora = Convert.ToDecimal(reader["TarifaBasePorHora"])
                        };
                    }
                }
            }

            return null;
        }
    }

    public class ReservaDatos
    {
        public ReservaDatos()
        {
            Conexion.Inicializar();
        }

        private const string ColumnasSelect =
            "Reserva.Id, Reserva.ClienteId, Reserva.PuestoId, Reserva.FechaInicio, " +
            "Reserva.FechaFin, Reserva.Estado, Reserva.CostoTotal";

        private Reserva LeerFila(SqliteDataReader reader)
        {
            return new Reserva
            {
                Id = Convert.ToInt32(reader["Id"]),
                ClienteId = Convert.ToInt32(reader["ClienteId"]),
                PuestoId = Convert.ToInt32(reader["PuestoId"]),
                FechaInicio = DateTime.ParseExact(reader["FechaInicio"].ToString() ?? "", Conexion.FormatoFecha, CultureInfo.InvariantCulture),
                FechaFin = DateTime.ParseExact(reader["FechaFin"].ToString() ?? "", Conexion.FormatoFecha, CultureInfo.InvariantCulture),
                Estado = Enum.Parse<EstadoReserva>(reader["Estado"].ToString() ?? ""),
                CostoTotal = Convert.ToDecimal(reader["CostoTotal"])
            };
        }

        // devuelve el Id de la reserva nueva, o 0 si fallo
        public int Add(Reserva reserva)
        {
            string query = "INSERT INTO Reserva (ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal) " +
                           "VALUES (@ClienteId, @PuestoId, @FechaInicio, @FechaFin, @Estado, @CostoTotal); " +
                           "SELECT last_insert_rowid();";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                conexion.Open();

                using (SqliteTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        SqliteCommand comando = new SqliteCommand(query, conexion, transaccion);
                        comando.Parameters.AddWithValue("@ClienteId", reserva.ClienteId);
                        comando.Parameters.AddWithValue("@PuestoId", reserva.PuestoId);
                        comando.Parameters.AddWithValue("@FechaInicio", reserva.FechaInicio.ToString(Conexion.FormatoFecha, CultureInfo.InvariantCulture));
                        comando.Parameters.AddWithValue("@FechaFin", reserva.FechaFin.ToString(Conexion.FormatoFecha, CultureInfo.InvariantCulture));
                        comando.Parameters.AddWithValue("@Estado", reserva.Estado.ToString());
                        comando.Parameters.AddWithValue("@CostoTotal", (double)reserva.CostoTotal);
                        int id = Convert.ToInt32(comando.ExecuteScalar());

                        transaccion.Commit();
                        return id;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        return 0;
                    }
                }
            }
        }

        public Reserva? ObtenerPorId(int id)
        {
            string query = $"SELECT {ColumnasSelect} FROM Reserva WHERE Reserva.Id = @Id";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                SqliteCommand comando = new SqliteCommand(query, conexion);
                comando.Parameters.AddWithValue("@Id", id);

                conexion.Open();

                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    if (reader.Read()) return LeerFila(reader);
                }
            }

            return null;
        }

        public bool Cancelar(int id)
        {
            string query = "UPDATE Reserva SET Estado = @Estado WHERE Id = @Id";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                conexion.Open();

                using (SqliteTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        SqliteCommand comando = new SqliteCommand(query, conexion, transaccion);
                        comando.Parameters.AddWithValue("@Estado", EstadoReserva.Cancelada.ToString());
                        comando.Parameters.AddWithValue("@Id", id);
                        int filas = comando.ExecuteNonQuery();

                        transaccion.Commit();
                        return filas > 0;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        return false;
                    }
                }
            }
        }

        // reservas confirmadas y futuras de un puesto (se busca por su codigo)
        public List<Reserva> ListarActivasPorPuesto(string codigoPuesto)
        {
            var resultados = new List<Reserva>();

            string query = $@"SELECT {ColumnasSelect} FROM Reserva
                            INNER JOIN Puesto ON Reserva.PuestoId = Puesto.Id
                            WHERE Puesto.Codigo = @Codigo
                            AND Reserva.Estado = 'Confirmada'
                            AND Reserva.FechaInicio > @Ahora
                            ORDER BY Reserva.FechaInicio";

            using (SqliteConnection conexion = new SqliteConnection(Conexion.CadenaConexion))
            {
                SqliteCommand comando = new SqliteCommand(query, conexion);
                comando.Parameters.AddWithValue("@Codigo", codigoPuesto);
                comando.Parameters.AddWithValue("@Ahora", DateTime.Now.ToString(Conexion.FormatoFecha, CultureInfo.InvariantCulture));

                conexion.Open();

                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        resultados.Add(LeerFila(reader));
                    }
                }
            }

            return resultados;
        }
    }
}
