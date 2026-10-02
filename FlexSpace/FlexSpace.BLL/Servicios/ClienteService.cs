using FlexSpace.BLL.Dtos;
using FlexSpace.DAL.Repositories;

namespace FlexSpace.BLL.Servicios;

public class ClienteService
{
    private readonly ClienteRepository _clientes;
    public ClienteService(ClienteRepository clientes) => _clientes = clientes;

    public List<ClienteSancionadoDto> ListarSancionados() =>
        _clientes.ListarConSanciones()
            .Select(c => new ClienteSancionadoDto(
                c.Id, c.Nombre, c.Email, c.TipoCliente.ToString(),
                c.SancionesActivas, c.SancionesActivas >= ReservaService.MaxSanciones))
            .ToList();
}
