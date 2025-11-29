using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services
{
    public class ServiceEspecialidad : IServiceEspecialidad
    {
        private readonly IRepositoryEspecialidad _repo;
        private readonly IMapper _mapper;

        public ServiceEspecialidad(IRepositoryEspecialidad repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<EspecialidadDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<List<EspecialidadDTO>>(list);
        }

        public async Task<EspecialidadDTO?> FindByIdAsync(int id)
        {
            var entity = await _repo.FindByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<EspecialidadDTO>(entity);
        }

        public async Task<int> AddAsync(EspecialidadDTO dto)
        {
            var entity = _mapper.Map<Especialidad>(dto);
            return await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(EspecialidadDTO dto)
        {
            var entity = await _repo.FindByIdAsync(dto.EspecialidadId);
            if (entity == null) throw new System.ArgumentException("Especialidad no encontrada");
            entity.Nombre = dto.Nombre;
            entity.Activa = dto.Activa;
            await _repo.UpdateAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
