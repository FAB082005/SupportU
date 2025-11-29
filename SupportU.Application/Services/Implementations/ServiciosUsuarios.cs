using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infrastructure.Repository.Interfaces;
using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
    public class ServiceUsuario : IServiceUsuario
    {
        private readonly IRepositoryUsuario _repository;
        private readonly IMapper _mapper;

        public ServiceUsuario(IRepositoryUsuario repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> FindByIdAsync(int id)
        {
            var entity = await _repository.FindByIdAsync(id);
            var dto = _mapper.Map<UsuarioDTO>(entity);
            return dto;
        }

        public async Task<ICollection<UsuarioDTO>> ListAsync()
        {
            var list = await _repository.ListAsync();
            var collection = _mapper.Map<ICollection<UsuarioDTO>>(list);
            return collection;
        }

        public async Task<int> AddAsync(UsuarioDTO dto)
        {
            dto.FechaCreacion = dto.FechaCreacion == default ? DateTime.UtcNow : dto.FechaCreacion;
            dto.Activo = true;


            var entity = _mapper.Map<Usuario>(dto);

            if (string.IsNullOrWhiteSpace(entity.Email))
            {
                System.Diagnostics.Debug.WriteLine("Service.AddAsync: Email inválido después del mapeo");
                throw new ArgumentException("Email es requerido");
            }

            var id = await _repository.AddAsync(entity);
            return id;
        }

        public async Task UpdateAsync(UsuarioDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var entity = await _repository.FindByIdAsync(dto.UsuarioId);
            if (entity == null) throw new ArgumentException("Usuario no encontrado");

            entity.Email = dto.Email;
            entity.Nombre = dto.Nombre;
            entity.Apellidos = dto.Apellidos;
            entity.Rol = dto.Rol;
            entity.Activo = dto.Activo;
            entity.FechaCreacion = dto.FechaCreacion;

            // Solo re-hashear si se pidió cambiar contraseña
            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
            {
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
                entity.PasswordHash = hasher.HashPassword(null, dto.PasswordHash);
            }

            await _repository.UpdateAsync();
        }
        private bool VerifyPassword(string plain, string hashed)
        {
            if (string.IsNullOrEmpty(hashed)) return false;
            return plain == hashed;
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
