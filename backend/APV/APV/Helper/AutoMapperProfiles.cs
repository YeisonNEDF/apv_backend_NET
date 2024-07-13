using APV.DTOs.PacienteDto;
using APV.DTOs.VeterinarioDto;
using APV.Entities;
using AutoMapper;

namespace APV.Helper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {

            CreateMap<Veterinario, VeterinarioDTO>().ReverseMap();
            CreateMap<Veterinario, NuevoPasswordDTO>().ReverseMap();

            CreateMap<Paciente,  PacienteDTO>().ReverseMap();
            CreateMap<CrearPacienteDTO, Paciente>();
        }
    }
}
