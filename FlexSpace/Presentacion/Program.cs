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
                    Console.Write("Id de cliente: ");
                    int.TryParse(Console.ReadLine(), out int clienteId);
                    Console.Write("Id de puesto: ");
                    int.TryParse(Console.ReadLine(), out int puestoId);
                    Console.Write("Fecha/hora de inicio (YYYY-MM-DD HH:mm): ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime inicio);
                    Console.Write("Fecha/hora de fin (YYYY-MM-DD HH:mm): ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime fin);

                    try
                    {
                        // primero se muestra el resumen del precio, todavia no se guarda nada
                        ResumenReserva resumen = reservaNegocio.CalcularResumen(clienteId, puestoId, inicio, fin);

                        Console.WriteLine("--- Resumen de la reserva ---");
                        Console.WriteLine($"Cliente: {resumen.ClienteNombre} ({resumen.TipoCliente}, sanciones: {resumen.SancionesActivas})");
                        Console.WriteLine($"Puesto: {resumen.PuestoCodigo}");
                        Console.WriteLine($"Desde: {resumen.FechaInicio}  Hasta: {resumen.FechaFin}");
                        Console.WriteLine($"Horas: {resumen.Horas:0.##} x ${resumen.TarifaBasePorHora:0.00}");
                        Console.WriteLine($"Subtotal: ${resumen.Subtotal:0.00}");
                        for (int i = 0; i < resumen.Detalle.Count; i++)
                        {
                            Console.WriteLine($"  {resumen.Detalle[i].Descripcion}: ${resumen.Detalle[i].Monto:0.00}");
                        }
                        Console.WriteLine($"TOTAL: ${resumen.CostoTotal:0.00}");

                        Console.Write("Confirmar reserva? (s/n): ");
                        if (Console.ReadLine()?.Trim().ToLower() == "s")
                        {
                            int idNuevo = reservaNegocio.RegistrarReserva(clienteId, puestoId, inicio, fin);
                            Console.WriteLine(idNuevo > 0 ? $"Reserva registrada con exito. Id: {idNuevo}" : "No se pudo registrar.");
                        }
                        else
                        {
                            Console.WriteLine("Reserva descartada.");
                        }
                    }
                    catch (NegocioException ex)
                    {
                        Console.WriteLine("No se pudo registrar: " + ex.Message);
                    }
                    break;

                case "2":
                    Console.Write("Ingrese el Id de la reserva a cancelar: ");
                    int.TryParse(Console.ReadLine(), out int idCancelar);

                    try
                    {
                        bool cancelada = reservaNegocio.CancelarReserva(idCancelar, out bool sancion);
                        Console.WriteLine(cancelada ? "Cancelada con exito." : "No se pudo cancelar.");
                        if (sancion)
                        {
                            Console.WriteLine("Se cancelo con menos de 2 horas de anticipacion: se sumo 1 sancion al cliente.");
                        }
                    }
                    catch (NegocioException ex)
                    {
                        Console.WriteLine("No se pudo cancelar: " + ex.Message);
                    }
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
