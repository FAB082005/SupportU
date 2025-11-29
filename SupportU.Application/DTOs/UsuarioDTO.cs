using System;
using System.ComponentModel.DataAnnotations;

namespace SupportU.Application.DTOs
{
    public record UsuarioDTO
    {
        public int UsuarioId { get; set; }

        public string Email { get; set; }

        public string? PasswordHash { get; set; }

        public string Nombre { get; set; }

        public string Apellidos { get; set; }

        public string Rol { get; set; }

        public DateTime? UltimoInicioSesion { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
