using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
	public class ServiceValoracion : IServiceValoracion
	{
		private readonly IRepositoryValoracion _repo;
		private readonly IMapper _mapper;

		public ServiceValoracion(IRepositoryValoracion repo, IMapper mapper)
		{
			_repo = repo;
			_mapper = mapper;
		}

		public async Task<List<ValoracionDTO>> ListAsync()
		{
			var list = await _repo.ListAsync();
			return _mapper.Map<List<ValoracionDTO>>(list);
		}

		public async Task<ValoracionDTO?> GetByIdAsync(int id)
		{
			var valoracion = await _repo.GetByIdAsync(id);
			return valoracion != null ? _mapper.Map<ValoracionDTO>(valoracion) : null;
		}

		public async Task<ValoracionDTO?> GetByTicketIdAsync(int ticketId)
		{
			var valoracion = await _repo.GetByTicketIdAsync(ticketId);
			return valoracion != null ? _mapper.Map<ValoracionDTO>(valoracion) : null;
		}

		public async Task<ValoracionDTO> CreateAsync(ValoracionDTO dto)
		{
			// Validaciones
			if (dto.TicketId <= 0)
				throw new ArgumentException("Debe seleccionar un ticket válido");

			if (dto.UsuarioId <= 0)
				throw new ArgumentException("Debe especificar un usuario válido");

			if (dto.Puntaje < 1 || dto.Puntaje > 5)
				throw new ArgumentException("El puntaje debe estar entre 1 y 5");

			// Verificar que no exista ya una valoración para este ticket
			if (await _repo.ExistsForTicketAsync(dto.TicketId))
				throw new InvalidOperationException("Este ticket ya tiene una valoración");

			var valoracion = _mapper.Map<Valoracion>(dto);
			valoracion.FechaValoracion = DateTime.Now;

			var created = await _repo.CreateAsync(valoracion);
			return _mapper.Map<ValoracionDTO>(created);
		}

		public async Task<bool> CanCreateValoracionAsync(int ticketId)
		{
			return !await _repo.ExistsForTicketAsync(ticketId);
		}

		public async Task<List<ValoracionDTO>> GetByUsuarioIdAsync(int usuarioId)
		{
			var list = await _repo.GetByUsuarioIdAsync(usuarioId);
			return _mapper.Map<List<ValoracionDTO>>(list);
		}
	}
}