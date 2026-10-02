using FlexSpace.BLL.Dtos;
using FlexSpace.BLL.Excepciones;
using FlexSpace.BLL.Tarifas;
using FlexSpace.DAL.Entities;
using FlexSpace.DAL.Repositories;

namespace FlexSpace.BLL.Servicios;

public class ReservaService
{
    public const int MaxSanciones = 3;

    private readonly ClienteRepository _clientes;
    private readonly PuestoRepository _puestos;
    private readonly ReservaRepository _reservas;
    private readonly CalculadorTarifa _calculador = new();

    public ReservaService(ClienteRepository clientes, PuestoRepository puestos, ReservaRepository reservas)
    {
        _clientes = clientes;
        _puestos = puestos;
        _reservas = reservas;
    }

    /// <summary>Valida todo y calcula el precio SIN persistir (para mostrar el resumen antes de confirmar).</summary>
    public ResumenReservaDto Previsualizar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
    {
        var (cliente, puesto, resultado) = Validar(clienteId, puestoId, inicio, fin);
        return new ResumenReservaDto(
            cliente.Nombre, cliente.TipoCliente.ToString(), cliente.SancionesActivas,
            puesto.Codigo, inicio, fin,
            Horas(inicio, fin), puesto.TarifaBasePorHora,
            resultado.Subtotal, resultado.Pasos, resultado.Total);
    }

    /// <summary>Revalida (por si cambio algo) y persiste la reserva. Devuelve el Id generado.</summary>
    public int Confirmar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
    {
        var (_, _, resultado) = Validar(clienteId, puestoId, inicio, fin);
        return _reservas.Insertar(new Reserva
        {
            ClienteId = clienteId,
            PuestoId = puestoId,
            FechaInicio = inicio,
            FechaFin = fin,
            Estado = EstadoReserva.Confirmada,
            CostoTotal = resultado.Total
        });
    }

    private (Cliente, Puesto, ResultadoTarifa) Validar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new ReservaInvalidaException("La fecha de fin debe ser posterior a la de inicio.");
        if (inicio < DateTime.Now)
            throw new ReservaInvalidaException("No se pueden crear reservas en el pasado.");

        var cliente = _clientes.ObtenerPorId(clienteId)
                      ?? throw new EntidadNoEncontradaException("cliente", clienteId);
        var puesto = _puestos.ObtenerPorId(puestoId)
                     ?? throw new EntidadNoEncontradaException("puesto", puestoId);

        if (cliente.SancionesActivas >= MaxSanciones)
            throw new ClienteSancionadoException(cliente.Nombre, cliente.SancionesActivas);

        if (_reservas.ExisteSolapamiento(puestoId, inicio, fin))
            throw new PuestoNoDisponibleException(puesto.Codigo);

        var ctx = new ContextoTarifa(
            Horas(inicio, fin),
            puesto.TarifaBasePorHora,
            CalculadorTarifa.IncluyeFinDeSemana(inicio, fin),
            cliente.TipoCliente == TipoCliente.VIP,
            cliente.SancionesActivas > 0);

        return (cliente, puesto, _calculador.Calcular(ctx));
    }

    private static decimal Horas(DateTime inicio, DateTime fin) => (decimal)(fin - inicio).TotalHours;
}
