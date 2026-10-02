namespace FlexSpace.BLL.Dtos;

public record PasoTarifaDto(string Descripcion, decimal Monto);

public record ResumenReservaDto(
    string ClienteNombre,
    string TipoCliente,
    int SancionesActivas,
    string PuestoCodigo,
    DateTime Inicio,
    DateTime Fin,
    decimal Horas,
    decimal TarifaBasePorHora,
    decimal Subtotal,
    IReadOnlyList<PasoTarifaDto> Pasos,
    decimal CostoTotal);

public record ClienteSancionadoDto(int Id, string Nombre, string Email, string TipoCliente, int Sanciones, bool Bloqueado);
