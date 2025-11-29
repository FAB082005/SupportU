using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Asignacion
{
    public int AsignacionId { get; set; }

    public int TicketId { get; set; }

    public int TecnicoId { get; set; }

    public string MetodoAsignacion { get; set; } = null!;

    public DateTime FechaAsignacion { get; set; }

    public int UsuarioAsignadorId { get; set; }

    public virtual Tecnico Tecnico { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Usuario UsuarioAsignador { get; set; } = null!;
}
