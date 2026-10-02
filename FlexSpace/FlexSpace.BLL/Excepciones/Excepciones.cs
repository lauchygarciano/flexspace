namespace FlexSpace.BLL.Excepciones;

/// <summary>Base de todas las excepciones de negocio (la UI las captura y muestra el mensaje).</summary>
public class FlexSpaceException : Exception
{
    public FlexSpaceException(string mensaje) : base(mensaje) { }
}

public class ClienteSancionadoException : FlexSpaceException
{
    public ClienteSancionadoException(string cliente, int sanciones)
        : base($"El cliente '{cliente}' tiene {sanciones} sanciones activas y no puede reservar.") { }
}

public class PuestoNoDisponibleException : FlexSpaceException
{
    public PuestoNoDisponibleException(string codigo)
        : base($"El puesto {codigo} ya tiene una reserva confirmada que se solapa con ese horario.") { }
}

public class EntidadNoEncontradaException : FlexSpaceException
{
    public EntidadNoEncontradaException(string entidad, int id)
        : base($"No existe {entidad} con Id {id}.") { }
}

public class ReservaInvalidaException : FlexSpaceException
{
    public ReservaInvalidaException(string mensaje) : base(mensaje) { }
}
