using System;
using System.Collections.Generic;
using Datos;
using Entidades;

namespace Negocio
{

    public class ReservaNegocio
    {
        private ReservaDatos _datos = new ReservaDatos();

        public int RegistrarReserva(Reserva reserva)
        {
            if (reserva == null) return 0;

            // TODO reglas de negocio (entrega final):
            // - validar disponibilidad sin solapamiento
            // - calcular el costo total con las reglas de tarifa
            // - rechazar clientes con 3 o mas sanciones (ClienteSancionadoException)
            reserva.Estado = EstadoReserva.Confirmada;
            reserva.CostoTotal = 0;

            return _datos.Add(reserva);
        }

        public bool CancelarReserva(int id)
        {
            if (id <= 0) return false;

            // TODO reglas de negocio (entrega final):
            // - si faltan menos de 2 horas para el inicio, sumar +1 a las sanciones del cliente
            return _datos.Cancelar(id);
        }

        public List<Reserva> ObtenerActivasPorPuesto(string codigoPuesto)
        {
            if (string.IsNullOrWhiteSpace(codigoPuesto)) return new List<Reserva>();

            return _datos.ListarActivasPorPuesto(codigoPuesto);
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
