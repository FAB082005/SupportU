using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryEtiqueta
    {
        Task<List<Etiqueta>> ListAsync();
    }
}
