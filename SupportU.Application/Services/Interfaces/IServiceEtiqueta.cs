using SupportU.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Interfaces
{
    public interface IServiceEtiqueta
    {
        Task<List<EtiquetaDTO>> ListAsync();
    }
}
