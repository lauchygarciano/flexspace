using FlexSpace.BLL.Dtos;

namespace FlexSpace.BLL.Tarifas;

public record ContextoTarifa(decimal Horas, decimal TarifaBase, bool IncluyeFinDeSemana, bool EsVip, bool TieneSanciones)
{
    public decimal Subtotal => Horas * TarifaBase;
}

/// <summary>Regla de tarifa (polimorfismo): cada regla decide si aplica y cuanto suma/resta.</summary>
public abstract class ReglaTarifa
{
    public abstract PasoTarifaDto? Aplicar(ContextoTarifa ctx, decimal acumulado);
}

public class RecargoFinDeSemana : ReglaTarifa
{
    public override PasoTarifaDto? Aplicar(ContextoTarifa ctx, decimal acumulado) =>
        ctx.IncluyeFinDeSemana
            ? new PasoTarifaDto("Recargo fin de semana (+15% del subtotal)", ctx.Subtotal * 0.15m)
            : null;
}

public class DescuentoPorVolumen : ReglaTarifa
{
    public override PasoTarifaDto? Aplicar(ContextoTarifa ctx, decimal acumulado) =>
        ctx.Horas >= 5
            ? new PasoTarifaDto("Descuento por volumen (-10% del acumulado)", -acumulado * 0.10m)
            : null;
}

public class DescuentoVip : ReglaTarifa
{
    public override PasoTarifaDto? Aplicar(ContextoTarifa ctx, decimal acumulado) =>
        ctx.EsVip
            ? new PasoTarifaDto("Beneficio VIP (-5% extra)", -acumulado * 0.05m)
            : null;
}

public class RecargoPorSanciones : ReglaTarifa
{
    public override PasoTarifaDto? Aplicar(ContextoTarifa ctx, decimal acumulado) =>
        ctx.TieneSanciones
            ? new PasoTarifaDto("Penalizacion por sanciones (+20% sobre tarifa base)", ctx.Subtotal * 0.20m)
            : null;
}
