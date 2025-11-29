using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services
{
    public class ServiceSla : IServiceSla
    {
        private readonly IRepositorySla _repo;
        private readonly IMapper _mapper;

        public ServiceSla(IRepositorySla repo, IMapper mapper)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SlaDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<List<SlaDTO>>(list);
        }

        public async Task<SlaDTO?> FindByIdAsync(int id)
        {
            var entity = await _repo.FindByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SlaDTO>(entity);
        }

        public async Task<int> AddAsync(SlaDTO dto)
        {
            var entity = _mapper.Map<Sla>(dto);
            entity.Activo = true;
            return await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(SlaDTO dto)
        {
            var entity = await _repo.FindByIdAsync(dto.SlaId);
            if (entity == null) throw new ArgumentException("SLA no encontrado");
            entity.Nombre = dto.Nombre;
            entity.TiempoRespuestaMinutos = dto.TiempoRespuestaMinutos;
            entity.TiempoResolucionMinutos = dto.TiempoResolucionMinutos;
            entity.Activo = dto.Activo;
            await _repo.UpdateAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
