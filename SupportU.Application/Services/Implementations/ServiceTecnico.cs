using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using SupportU.Infrastructure.Repository;
namespace SupportU.Application.Services
{
	public class ServiceTecnico : IServiceTecnico
	{
		private readonly IRepositoryTecnico _repo;
		private readonly IMapper _mapper;
        private readonly ILogger<ServiceTecnico> _logger;
        public ServiceTecnico(IRepositoryTecnico repo, IMapper mapper, ILogger<ServiceTecnico> logger)
		{
			_repo = repo;
			_mapper = mapper;
			_logger = logger;

        }

		public async Task<List<TecnicoDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			return _mapper.Map<List<TecnicoDTO>>(list);
		}

		public async Task<TecnicoDTO?> FindByIdAsync(int id)
		{
			var tecnico = await _repo.FindByIdAsync(id);
			if (tecnico == null) return null;
			return _mapper.Map<TecnicoDTO>(tecnico);
		}

		public async Task<TecnicoDTO?> FindByUsuarioIdAsync(int usuarioId)
		{
			var tecnicos = await _repo.ListAsync();
			var tecnico = tecnicos.FirstOrDefault(t => t.UsuarioId == usuarioId);
			return _mapper.Map<TecnicoDTO>(tecnico);
		}

		public async Task IncrementarCargaAsync(int tecnicoId)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null)
			{
				tecnico.CargaTrabajo++;
				await _repo.UpdateAsync(tecnico);
			}
		}

		public async Task DecrementarCargaAsync(int tecnicoId)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null && tecnico.CargaTrabajo > 0)
			{
				tecnico.CargaTrabajo--;
				await _repo.UpdateAsync(tecnico);
			}
		}

		public async Task ActualizarEstadoAsync(int tecnicoId, string nuevoEstado)
		{
			var tecnico = await _repo.FindByIdAsync(tecnicoId);
			if (tecnico != null)
			{
				tecnico.Estado = nuevoEstado;
				await _repo.UpdateAsync(tecnico);
			}
		}
        public async Task UpdateEspecialidadesAsync(int tecnicoId, List<int> especialidadIds)
        {
            if (especialidadIds == null) especialidadIds = new List<int>();
            await _repo.UpdateEspecialidadesAsync(tecnicoId, especialidadIds);
            _logger.LogInformation("ServiceTecnico.UpdateEspecialidadesAsync tecnicoId={Id} count={Count}", tecnicoId, especialidadIds.Count);
        }

        public async Task<int> AddAsync(TecnicoDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.UsuarioId <= 0) throw new ArgumentException("UsuarioId inválido en TecnicoDTO");

            var entity = new Tecnico
            {
                UsuarioId = dto.UsuarioId,
                CargaTrabajo = dto.CargaTrabajo,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Disponible" : dto.Estado,
                CalificacionPromedio = dto.CalificacionPromedio
            };

            var newId = await _repo.AddAsync(entity);

            if (newId <= 0) throw new InvalidOperationException("No se pudo crear el técnico (id inválido).");

            if (dto.EspecialidadIds != null && dto.EspecialidadIds.Count > 0)
            {
                await _repo.UpdateEspecialidadesAsync(newId, dto.EspecialidadIds);
            }

            return newId;
        }
    }
}