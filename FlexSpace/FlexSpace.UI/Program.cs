using System.Globalization;
using FlexSpace.BLL;
using FlexSpace.BLL.Dtos;
using FlexSpace.BLL.Excepciones;

const string ConnectionString = "Data Source=flexspace.db";
const string FormatoFecha = "dd/MM/yyyy HH:mm";

Bootstrapper.InicializarBaseDeDatos(ConnectionString);
var reservaService = Bootstrapper.CrearReservaService(ConnectionString);
var clienteService = Bootstrapper.CrearClienteService(ConnectionString);

while (true)
{
    Console.WriteLine();
    Console.WriteLine("===== FlexSpace =====");
    Console.WriteLine("1. Registrar nueva reserva");
    Console.WriteLine("2. Cancelar reserva");
    Console.WriteLine("3. Consultar reservas activas por puesto");
    Console.WriteLine("4. Listar clientes sancionados");
    Console.WriteLine("0. Salir");
    Console.Write("Opcion: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": RegistrarReserva(); break;
        case "2": Pendiente("Cancelar reserva"); break;
        case "3": Pendiente("Consultar reservas activas por puesto"); break;
        case "4": ListarSancionados(); break;
        case "0": return;
        default: Console.WriteLine("Opcion invalida."); break;
    }
}

void RegistrarReserva()
{
    try
    {
        int clienteId = LeerEntero("Id de cliente: ");
        int puestoId = LeerEntero("Id de puesto: ");
        DateTime inicio = LeerFecha("Inicio (dd/MM/yyyy HH:mm): ");
        DateTime fin = LeerFecha("Fin    (dd/MM/yyyy HH:mm): ");

        ResumenReservaDto r = reservaService.Previsualizar(clienteId, puestoId, inicio, fin);
        MostrarResumen(r);

        Console.Write("Confirmar reserva? (s/n): ");
        if (Console.ReadLine()?.Trim().ToLower() != "s")
        {
            Console.WriteLine("Reserva descartada.");
            return;
        }
        int id = reservaService.Confirmar(clienteId, puestoId, inicio, fin);
        Console.WriteLine($"Reserva #{id} registrada correctamente.");
    }
    catch (FlexSpaceException ex)
    {
        Console.WriteLine($"[No se pudo registrar] {ex.Message}");
    }
}

void ListarSancionados()
{
    var lista = clienteService.ListarSancionados();
    if (lista.Count == 0) { Console.WriteLine("No hay clientes sancionados."); return; }

    Console.WriteLine($"{"Id",-4}{"Nombre",-20}{"Tipo",-10}{"Sanciones",-10}Estado");
    foreach (var c in lista)
        Console.WriteLine($"{c.Id,-4}{c.Nombre,-20}{c.TipoCliente,-10}{c.Sanciones,-10}{(c.Bloqueado ? "BLOQUEADO" : "Con penalizacion")}");
}

void MostrarResumen(ResumenReservaDto r)
{
    Console.WriteLine();
    Console.WriteLine("----- Resumen de la reserva -----");
    Console.WriteLine($"Cliente: {r.ClienteNombre} ({r.TipoCliente}, sanciones: {r.SancionesActivas})");
    Console.WriteLine($"Puesto : {r.PuestoCodigo}");
    Console.WriteLine($"Desde  : {r.Inicio:dd/MM/yyyy HH:mm}   Hasta: {r.Fin:dd/MM/yyyy HH:mm}");
    Console.WriteLine($"Horas  : {r.Horas:0.##} x ${r.TarifaBasePorHora:0.00}");
    Console.WriteLine($"Subtotal base: ${r.Subtotal:0.00}");
    foreach (var p in r.Pasos)
        Console.WriteLine($"  {p.Descripcion}: {(p.Monto >= 0 ? "+" : "-")}${Math.Abs(p.Monto):0.00}");
    Console.WriteLine($"TOTAL: ${r.CostoTotal:0.00}");
}

void Pendiente(string nombre) =>
    Console.WriteLine($"'{nombre}' aun no esta implementada (se completa en la entrega final).");

int LeerEntero(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        if (int.TryParse(Console.ReadLine(), out int n)) return n;
        Console.WriteLine("Ingrese un numero entero valido.");
    }
}

DateTime LeerFecha(string mensaje)
{
    while (true)
    {
        Console.Write(mensaje);
        if (DateTime.TryParseExact(Console.ReadLine()?.Trim(), FormatoFecha,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)) return f;
        Console.WriteLine($"Formato invalido. Use {FormatoFecha}.");
    }
}
