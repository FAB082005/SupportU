using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services
{
    public class ServiceTecnico : IServiceTecnico
    {
        private readonly IRepositoryTecnico _repo;
        private readonly IMapper _mapper;

        public ServiceTecnico(IRepositoryTecnico repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<TecnicoDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<List<TecnicoDTO>>(list);
        }
    }
}
