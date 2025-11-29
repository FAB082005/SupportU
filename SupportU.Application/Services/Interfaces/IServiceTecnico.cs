using SupportU.Application.DTOs;

namespace SupportU.Application.Services
{
    public interface IServiceTecnico
    {
        Task<List<TecnicoDTO>> ListAsync();
    }
}
