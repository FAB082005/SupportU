using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
    public class ServiceHistorialEstados : IServiceHistorialEstados
    {
        private readonly IRepositoryHistorialEstados _repo;
        private readonly IMapper _mapper;

        public ServiceHistorialEstados(IRepositoryHistorialEstados repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ICollection<HistorialEstadosDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<ICollection<HistorialEstadosDTO>>(list);
        }

        public async Task<HistorialEstadosDTO?> FindByIdAsync(int id)
        {
            var entity = await _repo.FindByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<HistorialEstadosDTO>(entity);
        }

        public async Task<int> AddAsync(HistorialEstadosDTO dto)
        {
            // Construcción manual de la entidad para evitar problemas con relaciones
            var entity = new HistorialEstado
            {
                TicketId = dto.TicketId,
                EstadoAnterior = dto.EstadoAnterior,
                EstadoNuevo = dto.EstadoNuevo,
                UsuarioId = dto.UsuarioId,
                Observaciones = dto.Observaciones,
                FechaCambio = dto.FechaCambio
            };

            return await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(HistorialEstadosDTO dto)
        {
            var entity = await _repo.FindByIdAsync(dto.HistorialId);
            if (entity == null) throw new KeyNotFoundException("Historial de estado no encontrado");

            // Actualizar solo los campos escalares
            entity.EstadoAnterior = dto.EstadoAnterior;
            entity.EstadoNuevo = dto.EstadoNuevo;
            entity.UsuarioId = dto.UsuarioId;
            entity.Observaciones = dto.Observaciones;
            entity.FechaCambio = dto.FechaCambio;
            entity.TicketId = dto.TicketId;

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            
        }
    }
}
