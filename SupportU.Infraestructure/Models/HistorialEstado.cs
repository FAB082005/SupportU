using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class HistorialEstado
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
    public virtual ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
}
