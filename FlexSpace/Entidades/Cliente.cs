namespace Entidades
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public TipoCliente TipoCliente { get; set; }
        public int SancionesActivas { get; set; }
    }
}
