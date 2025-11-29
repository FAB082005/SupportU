using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Infraestructure.Repository.Interfaces
{
    public interface IRepositoryAsignacion
    {
        Task<ICollection<Asignacion>> ListAsync();
        Task<Asignacion> FindByIdAsync(int id);
        Task<ICollection<Asignacion>> ListByTecnicoSemanaAsync(int tecnicoId, DateTime inicioSemana, DateTime finSemana);
    }

}
