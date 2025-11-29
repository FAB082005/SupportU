using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime? UltimoInicioSesion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Asignacion> Asignacion { get; set; } = new List<Asignacion>();

    public virtual ICollection<HistorialEstado> HistorialEstado { get; set; } = new List<HistorialEstado>();

    public virtual ICollection<Notificacion> Notificacion { get; set; } = new List<Notificacion>();

    public virtual Tecnico? Tecnico { get; set; }

    public virtual ICollection<Ticket> Ticket { get; set; } = new List<Ticket>();

    public virtual ICollection<Valoracion> Valoracion { get; set; } = new List<Valoracion>();
}
