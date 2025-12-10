using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportU.Infraestructure.Models;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int SlaId { get; set; }

    public string CriterioAsignacion { get; set; } = null!;

    public bool Activa { get; set; }

    public virtual ICollection<Etiqueta> Etiqueta { get; set; } = new List<Etiqueta>();

    public virtual Sla Sla { get; set; } = null!;

    public virtual ICollection<Ticket> Ticket { get; set; } = new List<Ticket>();

	public virtual ICollection<CategoriaEspecialidad> CategoriaEspecialidades { get; set; } = new List<CategoriaEspecialidad>();

	
}
