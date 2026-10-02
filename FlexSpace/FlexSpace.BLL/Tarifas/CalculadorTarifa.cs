using FlexSpace.BLL.Dtos;

namespace FlexSpace.BLL.Tarifas;

public record ResultadoTarifa(decimal Subtotal, IReadOnlyList<PasoTarifaDto> Pasos, decimal Total);

public class CalculadorTarifa
{
    // Cliente sin sanciones: fin de semana -> volumen -> VIP (en ese orden).
    private static readonly ReglaTarifa[] ReglasNormales =
        { new RecargoFinDeSemana(), new DescuentoPorVolumen(), new DescuentoVip() };

    // Cliente con sanciones: pierde descuentos (volumen y VIP) y paga +20% sobre la base.
    private static readonly ReglaTarifa[] ReglasSancionado =
        { new RecargoFinDeSemana(), new RecargoPorSanciones() };

    public ResultadoTarifa Calcular(ContextoTarifa ctx)
    {
        var reglas = ctx.TieneSanciones ? ReglasSancionado : ReglasNormales;
        decimal acumulado = ctx.Subtotal;
        var pasos = new List<PasoTarifaDto>();

        foreach (var regla in reglas)
        {
            var paso = regla.Aplicar(ctx, acumulado);
            if (paso is null) continue;
            acumulado += paso.Monto;
            pasos.Add(paso);
        }
        return new ResultadoTarifa(ctx.Subtotal, pasos, Math.Round(acumulado, 2));
    }

    /// <summary>True si el rango [inicio, fin) toca algun sabado o domingo.</summary>
    public static bool IncluyeFinDeSemana(DateTime inicio, DateTime fin)
    {
        for (var d = inicio; d < fin; d = d.Date.AddDays(1))
            if (d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return true;
        return false;
    }
}
