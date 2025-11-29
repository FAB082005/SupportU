using SupportU.Infraestructure.Models;

namespace SupportU.Application.DTOs
{
    public record TicketDTO
    {
        public int TicketId { get; set; }
        public string Titulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int UsuarioSolicitanteId { get; set; }
        public int CategoriaId { get; set; }
        public int? TecnicoAsignadoId { get; set; }
        public string Prioridad { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public bool? CumplimientoRespuesta { get; set; }
        public bool? CumplimientoResolucion { get; set; }
        public DateTime? fecha_primera_respuesta { get; set; }
        public DateTime? fecha_resolucion { get; set; }
        public string? ObservacionesIniciales { get; set; }
        public virtual ICollection<Asignacion> Asignacion { get; set; } = new List<Asignacion>();
        public virtual Categoria? Categoria { get; set; } = null!;
        public virtual ICollection<HistorialEstado> HistorialEstado { get; set; } = new List<HistorialEstado>();
        public virtual ICollection<Imagen> Imagen { get; set; } = new List<Imagen>();
        public virtual ICollection<Notificacion> Notificacion { get; set; } = new List<Notificacion>();
        public virtual Tecnico? TecnicoAsignado { get; set; }
        public virtual Usuario? UsuarioSolicitante { get; set; } = null!;
        public virtual Valoracion? Valoracion { get; set; }
    }
}