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
            // Mapeamento de Meta para MetaResponseDto
            CreateMap<Meta, MetaResponseDto>()
                .ForMember(dest => dest.PercentualConcluido, opt => opt.MapFrom(src =>
                    src.QuantidadeTotal > 0 ? (decimal)src.QuantidadeAtual / src.QuantidadeTotal * 100 : 0));

            // Mapeamento de MetaRequestDto para Meta
            CreateMap<MetaRequestDto, Meta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.QuantidadeAtual, opt => opt.Ignore())
                .ForMember(dest => dest.Concluida, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore());

            // Mapeamento de MetaUpdateRequestDto para Meta
            CreateMap<MetaUpdateRequestDto, Meta>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}