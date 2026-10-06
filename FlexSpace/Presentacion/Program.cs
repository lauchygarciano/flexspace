using Entidades;
using Negocio;

internal class Program
{
    private static void Main()
    {
        ReservaNegocio reservaNegocio = new ReservaNegocio();
        ClienteNegocio clienteNegocio = new ClienteNegocio();
        bool salir = false;

        while (!salir)
        {
            Console.WriteLine("--- FlexSpace ---");
            Console.WriteLine("1. Registrar nueva reserva");
            Console.WriteLine("2. Cancelar reserva");
            Console.WriteLine("3. Consultar reservas activas por puesto");
            Console.WriteLine("4. Listar clientes sancionados");
            Console.WriteLine("5. Salir");
            Console.Write("Opcion: ");
            string? opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    Reserva nueva = new Reserva();
                    Console.Write("Id de cliente: ");
                    int.TryParse(Console.ReadLine(), out int clienteId);
                    nueva.ClienteId = clienteId;
                    Console.Write("Id de puesto: ");
                    int.TryParse(Console.ReadLine(), out int puestoId);
                    nueva.PuestoId = puestoId;
                    Console.Write("Fecha/hora de inicio (YYYY-MM-DD HH:mm): ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime inicio);
                    nueva.FechaInicio = inicio;
                    Console.Write("Fecha/hora de fin (YYYY-MM-DD HH:mm): ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime fin);
                    nueva.FechaFin = fin;

                    int idNuevo = reservaNegocio.RegistrarReserva(nueva);
                    Console.WriteLine(idNuevo > 0 ? $"Reserva registrada con exito. Id: {idNuevo}" : "No se pudo registrar.");
                    break;

                case "2":
                    Console.Write("Ingrese el Id de la reserva a cancelar: ");
                    int.TryParse(Console.ReadLine(), out int idCancelar);
                    bool cancelada = reservaNegocio.CancelarReserva(idCancelar);
                    Console.WriteLine(cancelada ? "Cancelada con exito." : "No se pudo cancelar.");
                    break;

                case "3":
                    Console.Write("Ingrese el codigo del puesto: ");
                    string? codigo = Console.ReadLine();
                    List<Reserva> reservas = reservaNegocio.ObtenerActivasPorPuesto(codigo ?? "");

                    if (reservas.Count == 0)
                    {
                        Console.WriteLine("No hay reservas activas.");
                        break;
                    }

                    for (int i = 0; i < reservas.Count; i++)
                    {
                        Console.WriteLine($"Reserva Id: {reservas[i].Id}");
                        Console.WriteLine($"Cliente Id: {reservas[i].ClienteId}");
                        Console.WriteLine($"Inicio: {reservas[i].FechaInicio}");
                        Console.WriteLine($"Fin: {reservas[i].FechaFin}");
                        Console.WriteLine($"Estado: {reservas[i].Estado}");
                        Console.WriteLine($"Costo total: {reservas[i].CostoTotal}");
                    }
                    break;

                case "4":
                    List<Cliente> sancionados = clienteNegocio.ObtenerSancionados();

                    if (sancionados.Count == 0)
                    {
                        Console.WriteLine("No hay clientes sancionados.");
                        break;
                    }

                    for (int i = 0; i < sancionados.Count; i++)
                    {
                        Console.WriteLine($"Id: {sancionados[i].Id}");
                        Console.WriteLine($"Nombre: {sancionados[i].Nombre}");
                        Console.WriteLine($"Email: {sancionados[i].Email}");
                        Console.WriteLine($"Tipo: {sancionados[i].TipoCliente}");
                        Console.WriteLine($"Sanciones activas: {sancionados[i].SancionesActivas}");
                    }
                    break;

                case "5":
                    salir = true;
                    break;

                default:
                    Console.WriteLine("Opción invalida.");
                    break;
            }
        }
    }
}
