using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Sla
{
    public int SlaId { get; set; }

    public string Nombre { get; set; } = null!;

    public int TiempoRespuestaMinutos { get; set; }

    public int TiempoResolucionMinutos { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Categoria> Categoria { get; set; } = new List<Categoria>();
}
