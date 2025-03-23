using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;

namespace ERP_API.Mapping
{
    public class SessaoMappingProfile : Profile
    {
        public SessaoMappingProfile()
        {
            // Mapeamento de SessaoEstudo para SessaoEstudoResponseDto
            CreateMap<SessaoEstudo, SessaoEstudoResponseDto>();

            // Mapeamento de SessaoEstudoRequestDto para SessaoEstudo
            CreateMap<SessaoEstudoRequestDto, SessaoEstudo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.DataInicio, opt => opt.Ignore())
                .ForMember(dest => dest.DataFim, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.TempoEstudado, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore());

            // Mapeamento de PausaSessao para PausaResponseDto
            CreateMap<PausaSessao, PausaResponseDto>();

            // Mapeamento de PausaRequestDto para PausaSessao
            CreateMap<PausaRequestDto, PausaSessao>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.Inicio, opt => opt.Ignore())
                .ForMember(dest => dest.Fim, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore());
        }
    }
}