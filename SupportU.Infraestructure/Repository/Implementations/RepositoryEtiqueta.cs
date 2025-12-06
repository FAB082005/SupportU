using Microsoft.EntityFrameworkCore;
using SupportU.Infraestructure.Data;
using SupportU.Infraestructure.Models;
using SupportU.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Implementations
{
    public class RepositoryEtiqueta : IRepositoryEtiqueta
    {
        private readonly SupportUContext _context;

        public RepositoryEtiqueta(SupportUContext context) => _context = context;

        public async Task<List<Etiqueta>> ListAsync()
        {
            return await _context.Etiqueta
                .Include(e => e.Categoria) // Incluir la categoría
                .Where(e => e.Activa) // Solo etiquetas activas
                .ToListAsync();
        }
    }
}
