namespace Entidades
{
    public class Puesto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public TipoPuesto TipoPuesto { get; set; }
        public decimal TarifaBasePorHora { get; set; }
    }
}
