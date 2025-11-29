using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Notificacion
{
    public int NotificacionId { get; set; }

    public int UsuarioDestinatarioId { get; set; }

    public int? TicketId { get; set; }

    public string TipoNotificacion { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual Usuario UsuarioDestinatario { get; set; } = null!;
}
