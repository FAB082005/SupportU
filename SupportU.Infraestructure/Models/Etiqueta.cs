using System;
using System.Collections.Generic;

namespace SupportU.Infraestructure.Models;

public partial class Etiqueta
{
    public int EtiquetaId { get; set; }

    public string Nombre { get; set; } = null!;

    public int CategoriaId { get; set; }

    public bool Activa { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;
}
