using FlexSpace.BLL.Servicios;
using FlexSpace.DAL;
using FlexSpace.DAL.Repositories;

namespace FlexSpace.BLL;

/// <summary>
/// Punto de composicion: permite que la UI obtenga los servicios sin referenciar la DAL.
/// </summary>
public static class Bootstrapper
{
    public static void InicializarBaseDeDatos(string connectionString) =>
        DbInitializer.Inicializar(connectionString);

    public static ReservaService CrearReservaService(string cs) =>
        new(new ClienteRepository(cs), new PuestoRepository(cs), new ReservaRepository(cs));

    public static ClienteService CrearClienteService(string cs) =>
        new(new ClienteRepository(cs));
}
