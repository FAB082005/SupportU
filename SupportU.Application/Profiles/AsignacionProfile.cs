using AutoMapper;
using SupportU.Application.DTOs;
using SupportU.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportU.Application.Profiles
{
    public class AsignacionProfile : Profile
    {

        public AsignacionProfile()
        {
            CreateMap<AsignacionDTO, Asignacion>().ReverseMap();
        }

    }
}
