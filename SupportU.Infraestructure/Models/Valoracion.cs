using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Valoracion
{
    public int ValoracionId { get; set; }

    public int TicketId { get; set; }

    public int UsuarioId { get; set; }

    public int Puntaje { get; set; }

    public string? Comentario { get; set; }

    public DateTime FechaValoracion { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
