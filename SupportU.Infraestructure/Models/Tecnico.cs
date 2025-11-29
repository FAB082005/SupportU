using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Tecnico
{
    public int TecnicoId { get; set; }

    public int UsuarioId { get; set; }

    public int CargaTrabajo { get; set; }

    public string Estado { get; set; } = null!;

    public decimal CalificacionPromedio { get; set; }

    public virtual ICollection<Asignacion> Asignacion { get; set; } = new List<Asignacion>();

    public virtual ICollection<Ticket> Ticket { get; set; } = new List<Ticket>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<Especialidad> Especialidad { get; set; } = new List<Especialidad>();
}
