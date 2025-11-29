using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Application.Services.Interfaces;
using SupportU.Infraestructure.Repository.Interfaces;
using SupportU.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Services.Implementations
{
    public class ServiceEtiqueta : IServiceEtiqueta
    {

        private readonly IRepositoryEtiqueta _repo;
        private readonly IMapper _mapper;

        public ServiceEtiqueta(IRepositoryEtiqueta repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<EtiquetaDTO>> ListAsync()
        {
            var list = await _repo.ListAsync();
            return _mapper.Map<List<EtiquetaDTO>>(list);
        }

      
    }
}
