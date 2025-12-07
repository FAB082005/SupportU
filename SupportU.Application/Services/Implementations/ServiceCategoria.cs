using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository;

namespace SupportU.Application.Services
{
    public class ServiceCategoria : IServiceCategoria
    {
        private readonly IRepositoryCategoria _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceCategoria> _logger;

        public ServiceCategoria(IRepositoryCategoria repo, IMapper mapper, ILogger<ServiceCategoria> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CategoriaDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<List<CategoriaDTO>>(list);
        }

        public async Task<CategoriaDTO?> FindByIdAsync(int id)
        {
            var entity = await _repo.FindByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<CategoriaDTO>(entity);
        }

        public async Task<int> AddAsync(CategoriaDTO dto)
        {
            var entity = new Categoria
            {
                Nombre = dto.Nombre?.Trim() ?? string.Empty,
                Descripcion = dto.Descripcion,
                SlaId = dto.SlaId,
                CriterioAsignacion = dto.CriterioAsignacion ?? string.Empty,
                Activa = dto.Activa
            };

            var id = await _repo.AddAsync(entity);
            _logger.LogInformation("ServiceCategoria.AddAsync created CategoriaId={Id}", id);
            return id;
        }

        public async Task UpdateAsync(CategoriaDTO dto)
        {
            var entity = await _repo.FindByIdAsync(dto.CategoriaId);
            if (entity == null) throw new KeyNotFoundException("Categoría no encontrada");

            entity.Nombre = dto.Nombre?.Trim() ?? entity.Nombre;
            entity.Descripcion = dto.Descripcion;
            entity.SlaId = dto.SlaId;
            entity.CriterioAsignacion = dto.CriterioAsignacion ?? entity.CriterioAsignacion;
            entity.Activa = dto.Activa;

            await _repo.UpdateAsync(entity);
            _logger.LogInformation("ServiceCategoria.UpdateAsync updated CategoriaId={Id}", dto.CategoriaId);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
            _logger.LogInformation("ServiceCategoria.DeleteAsync deactivated CategoriaId={Id}", id);
        }
    }
}
