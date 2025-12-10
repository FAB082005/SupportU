using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.DTOs
{
    public class EtiquetaDTO
    {
        public int EtiquetaId { get; set; }

        public string Nombre { get; set; } = null!;

        public int CategoriaId { get; set; }

        public bool Activa { get; set; }

       // public virtual Categoria Categoria { get; set; } = null!;

    }
}
