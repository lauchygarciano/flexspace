namespace Entidades
{
    public enum TipoPuesto
    {
        EscritorioIndividual,
        SalaReuniones,
        CabinaPrivada
    }

    public class Puesto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public TipoPuesto TipoPuesto { get; set; }
        public decimal TarifaBasePorHora { get; set; }
    }
}
