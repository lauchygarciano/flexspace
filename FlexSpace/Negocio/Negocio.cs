using System;
using System.Collections.Generic;
using Datos;
using Entidades;

namespace Negocio
{
    
    public class NegocioException : Exception
    {
        public NegocioException(string mensaje) : base(mensaje) { }
    }

    public class ClienteSancionadoException : NegocioException
    {
        public ClienteSancionadoException(string nombre, int sanciones)
            : base($"El cliente {nombre} tiene {sanciones} sanciones activas y no puede reservar.") { }
    }

    public class ReservaInvalidaException : NegocioException
    {
        public ReservaInvalidaException(string mensaje) : base(mensaje) { }
    }

    //clases aux

    public class ConceptoTarifa
    {
        public string Descripcion { get; set; } = "";
        public decimal Monto { get; set; }
    }

    public class ResumenReserva
    {
        public string ClienteNombre { get; set; } = "";
        public string TipoCliente { get; set; } = "";
        public int SancionesActivas { get; set; }
        public string PuestoCodigo { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Horas { get; set; }
        public decimal TarifaBasePorHora { get; set; }
        public decimal Subtotal { get; set; }
        public List<ConceptoTarifa> Detalle { get; set; } = new List<ConceptoTarifa>();
        public decimal CostoTotal { get; set; }
    }

    // calc tarifa

    public class CalculadorTarifa
    {
        public decimal Calcular(Cliente cliente, Puesto puesto, DateTime inicio, DateTime fin, List<ConceptoTarifa> detalle)
        {
            decimal horas = (decimal)(fin - inicio).TotalHours;

            // 1
            decimal subtotal = horas * puesto.TarifaBasePorHora;
            decimal total = subtotal;

            // 2
            if (IncluyeFinDeSemana(inicio, fin))
            {
                decimal recargo = subtotal * 0.15m;
                total += recargo;
                detalle.Add(new ConceptoTarifa { Descripcion = "Recargo fin de semana (15%)", Monto = recargo });
            }

            if (cliente.SancionesActivas > 0)
            {
                // 5
                decimal penalizacion = subtotal * 0.20m;
                total += penalizacion;
                detalle.Add(new ConceptoTarifa { Descripcion = "Penalizacion por sanciones (20% sobre la base)", Monto = penalizacion });
            }
            else
            {
                // 3
                if (horas >= 5)
                {
                    decimal descuento = total * 0.10m;
                    total -= descuento;
                    detalle.Add(new ConceptoTarifa { Descripcion = "Descuento por volumen (10%)", Monto = -descuento });
                }

                // 4
                if (cliente.TipoCliente == TipoCliente.VIP)
                {
                    decimal descuentoVip = total * 0.05m;
                    total -= descuentoVip;
                    detalle.Add(new ConceptoTarifa { Descripcion = "Beneficio VIP (5%)", Monto = -descuentoVip });
                }
            }

            return Math.Round(total, 2);
        }

        // true si el rango de la reserva toca algun sabado o domingo
        private bool IncluyeFinDeSemana(DateTime inicio, DateTime fin)
        {
            for (DateTime dia = inicio; dia < fin; dia = dia.Date.AddDays(1))
            {
                if (dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // negocio

    public class ReservaNegocio
    {
        private const int MaximoSanciones = 3;
        private const int HorasMinimasCancelacion = 2;

        private ReservaDatos _reservaDatos = new ReservaDatos();
        private ClienteDatos _clienteDatos = new ClienteDatos();
        private PuestoDatos _puestoDatos = new PuestoDatos();
        private CalculadorTarifa _calculador = new CalculadorTarifa();

        
        public ResumenReserva CalcularResumen(int clienteId, int puestoId, DateTime inicio, DateTime fin)
        {
            if (fin <= inicio)
                throw new ReservaInvalidaException("La fecha de fin debe ser posterior a la de inicio.");

            if (inicio < DateTime.Now)
                throw new ReservaInvalidaException("No se puede reservar en una fecha pasada.");

            Cliente? cliente = _clienteDatos.ObtenerPorId(clienteId);
            if (cliente == null)
                throw new ReservaInvalidaException("No existe un cliente con ese Id.");

            Puesto? puesto = _puestoDatos.ObtenerPorId(puestoId);
            if (puesto == null)
                throw new ReservaInvalidaException("No existe un puesto con ese Id.");

            // regla c bloqueo de cliente
            if (cliente.SancionesActivas >= MaximoSanciones)
                throw new ClienteSancionadoException(cliente.Nombre, cliente.SancionesActivas);

            // regla a disponibilidad sin solapamiento
            if (_reservaDatos.ExisteSolapamiento(puestoId, inicio, fin))
                throw new ReservaInvalidaException($"El puesto {puesto.Codigo} ya tiene una reserva confirmada en ese horario.");

            // regla b calculo de la tarifa
            ResumenReserva resumen = new ResumenReserva();
            resumen.ClienteNombre = cliente.Nombre;
            resumen.TipoCliente = cliente.TipoCliente.ToString();
            resumen.SancionesActivas = cliente.SancionesActivas;
            resumen.PuestoCodigo = puesto.Codigo;
            resumen.FechaInicio = inicio;
            resumen.FechaFin = fin;
            resumen.Horas = (decimal)(fin - inicio).TotalHours;
            resumen.TarifaBasePorHora = puesto.TarifaBasePorHora;
            resumen.Subtotal = resumen.Horas * puesto.TarifaBasePorHora;
            resumen.CostoTotal = _calculador.Calcular(cliente, puesto, inicio, fin, resumen.Detalle);

            return resumen;
        }

        // vuelve a validar calcula y guarda la reserva 
        public int RegistrarReserva(int clienteId, int puestoId, DateTime inicio, DateTime fin)
        {
            ResumenReserva resumen = CalcularResumen(clienteId, puestoId, inicio, fin);

            Reserva reserva = new Reserva();
            reserva.ClienteId = clienteId;
            reserva.PuestoId = puestoId;
            reserva.FechaInicio = inicio;
            reserva.FechaFin = fin;
            reserva.Estado = EstadoReserva.Confirmada;
            reserva.CostoTotal = resumen.CostoTotal;

            return _reservaDatos.Add(reserva);
        }

        // cancela la reserva 
        public bool CancelarReserva(int id, out bool seAplicoSancion)
        {
            seAplicoSancion = false;

            Reserva? reserva = _reservaDatos.ObtenerPorId(id);
            if (reserva == null)
                throw new ReservaInvalidaException("No existe una reserva con ese Id.");

            if (reserva.Estado != EstadoReserva.Confirmada)
                throw new ReservaInvalidaException("Solo se pueden cancelar reservas confirmadas.");

            bool cancelada = _reservaDatos.Cancelar(id);

            if (cancelada && (reserva.FechaInicio - DateTime.Now).TotalHours < HorasMinimasCancelacion)
            {
                seAplicoSancion = _clienteDatos.SumarSancion(reserva.ClienteId);
            }

            return cancelada;
        }

        public List<Reserva> ObtenerActivasPorPuesto(string codigoPuesto)
        {
            if (string.IsNullOrWhiteSpace(codigoPuesto)) return new List<Reserva>();

            return _reservaDatos.ListarActivasPorPuesto(codigoPuesto);
        }
    }

    public class ClienteNegocio
    {
        private ClienteDatos _datos = new ClienteDatos();

        public List<Cliente> ObtenerSancionados()
        {
            return _datos.ListarSancionados();
        }
    }
}
