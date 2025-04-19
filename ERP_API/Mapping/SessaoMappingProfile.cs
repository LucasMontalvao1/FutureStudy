using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using ERP_API.Models.Enums;

namespace ERP_API.Mapping
{
    public class SessaoMappingProfile : Profile
    {
        public SessaoMappingProfile()
        {
            CreateMap<SessaoEstudo, SessaoEstudoResponseDto>();

            CreateMap<SessaoEstudoRequestDto, SessaoEstudo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.DataInicio, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DataFim, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusSessao.EmAndamento))
                .ForMember(dest => dest.TempoEstudado, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.NomeMateria, opt => opt.Ignore())
                .ForMember(dest => dest.NomeTopico, opt => opt.Ignore())
                .ForMember(dest => dest.NomeCategoria, opt => opt.Ignore())
                .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
                .ForMember(dest => dest.MateriaId, opt => opt.MapFrom(src => src.MateriaId))
                .ForMember(dest => dest.TopicoId, opt => opt.MapFrom(src => src.TopicoId));

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