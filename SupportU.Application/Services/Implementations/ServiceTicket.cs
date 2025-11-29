using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
    public class ServiceTicket : IServiceTicket
    {
        private readonly IRepositoryTicket _repository;
        private readonly IRepositoryCategoria _repositoryCategoria;
        private readonly IMapper _mapper;

        public ServiceTicket(
            IRepositoryTicket repository,
            IRepositoryCategoria repositoryCategoria,
            IMapper mapper)
        {
            _repository = repository;
            _repositoryCategoria = repositoryCategoria;
            _mapper = mapper;
        }

        public async Task<ICollection<TicketDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            var collection = _mapper.Map<List<TicketDTO>>(list);
            return collection;
        }

        public async Task<TicketDTO?> FindByIdAsync(int id)
        {
            var entity = await _repository.FindByIdAsync(id);
            if (entity == null) return null;

            var dto = _mapper.Map<TicketDTO>(entity);
            return dto;
        }

        public async Task<int> AddAsync(TicketDTO dto)
        {
            // Validar categoría
            var categoria = await _repositoryCategoria.FindByIdAsync(dto.CategoriaId);
            if (categoria == null)
                throw new KeyNotFoundException("Categoría no encontrada");

            // Construir la entidad del ticket
            var entity = new Ticket
            {
                Titulo = dto.Titulo?.Trim() ?? string.Empty,
                Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
                UsuarioSolicitanteId = dto.UsuarioSolicitanteId,
                CategoriaId = dto.CategoriaId,
                TecnicoAsignadoId = dto.TecnicoAsignadoId,
                Prioridad = dto.Prioridad ?? "Media",
                Estado = dto.Estado ?? "Pendiente",
                FechaCreacion = DateTime.Now,
                FechaCierre = null,
                CumplimientoRespuesta = null,
                CumplimientoResolucion = null,
                fecha_primera_respuesta = null,
                fecha_resolucion = null
            };

            // Guardar el ticket y retornar el ID
            var ticketId = await _repository.AddAsync(entity);
            return ticketId;
        }

        public async Task UpdateAsync(TicketDTO dto)
        {
      
            var entity = await _repository.FindByIdAsyncForUpdate(dto.TicketId);

            if (entity == null)
                throw new KeyNotFoundException("Ticket no encontrado");

            // Actualizar campos del ticket
            entity.Titulo = dto.Titulo?.Trim() ?? entity.Titulo;
            entity.Descripcion = dto.Descripcion?.Trim() ?? entity.Descripcion;
            entity.CategoriaId = dto.CategoriaId;
            entity.TecnicoAsignadoId = dto.TecnicoAsignadoId; 
            entity.Prioridad = dto.Prioridad ?? entity.Prioridad;
            entity.Estado = dto.Estado ?? entity.Estado;
            entity.FechaCierre = dto.FechaCierre;
            entity.CumplimientoRespuesta = dto.CumplimientoRespuesta;
            entity.CumplimientoResolucion = dto.CumplimientoResolucion;
            entity.fecha_primera_respuesta = dto.fecha_primera_respuesta;
            entity.fecha_resolucion = dto.fecha_resolucion;

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            // Implementar si es necesario
        }
    }
}