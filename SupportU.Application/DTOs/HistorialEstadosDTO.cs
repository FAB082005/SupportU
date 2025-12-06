using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
    public record HistorialEstadosDTO
    {
        public int HistorialId { get; set; }

        public int TicketId { get; set; }

        public string? EstadoAnterior { get; set; }

        public string EstadoNuevo { get; set; } = null!;

        public int UsuarioId { get; set; }

        public string? Observaciones { get; set; }

        public DateTime FechaCambio { get; set; }

        public virtual Ticket Ticket { get; set; } = null!;

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual ICollection<ImagenDTO> Imagenes { get; set; } = new List<ImagenDTO>();
    }
}
