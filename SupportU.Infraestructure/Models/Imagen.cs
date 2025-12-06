using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Imagen
{
    public int ImagenId { get; set; }

    public int? TicketId { get; set; }
    public int? HistorialEstadoId { get; set; }

    public string NombreArchivo { get; set; } = null!;

    public string RutaArchivo { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; } = null!;
    public virtual HistorialEstado? HistorialEstado { get; set; }
}
