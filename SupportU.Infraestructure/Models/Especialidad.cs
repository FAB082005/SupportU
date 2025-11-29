using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Especialidad
{
    public int EspecialidadId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activa { get; set; }

    public virtual ICollection<Categoria> Categoria { get; set; } = new List<Categoria>();

    public virtual ICollection<Tecnico> Tecnico { get; set; } = new List<Tecnico>();
}
