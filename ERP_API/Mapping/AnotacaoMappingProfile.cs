using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;

namespace ERP_API.Mapping
{
    public class AnotacaoMappingProfile : Profile
    {
        public AnotacaoMappingProfile()
        {
            // Mapeamento de Anotacao para AnotacaoResponseDto
            CreateMap<Anotacao, AnotacaoResponseDto>();

            // Mapeamento de AnotacaoRequestDto para Anotacao
            CreateMap<AnotacaoRequestDto, Anotacao>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore());

            // Mapeamento de AnotacaoUpdateDto para Anotacao
            CreateMap<AnotacaoUpdateDto, Anotacao>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UsuarioId, opt => opt.Ignore())
                .ForMember(dest => dest.SessaoId, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                .ForMember(dest => dest.AtualizadoEm, opt => opt.Ignore());
        }
    }
}