using AutoMapper;
using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Models.Enums;

namespace ERP_API.Mapping
{
    public class MetaMappingProfile : Profile
    {
        public MetaMappingProfile()
        {
            CreateMap<Meta, MetaResponseDto>()
                .ForMember(dest => dest.PercentualConcluido, opt => opt.MapFrom(src =>
                    src.QuantidadeTotal > 0 ? (src.QuantidadeAtual / src.QuantidadeTotal * 100) : 0));

            CreateMap<MetaRequestDto, Meta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.QuantidadeAtual, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.Concluida, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.UltimaVerificacao, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore());

            CreateMap<MetaUpdateRequestDto, Meta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}