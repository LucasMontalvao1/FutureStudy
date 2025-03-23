using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;

namespace ERP_API.Mapping
{
    public class HistoricoAnotacaoMappingProfile : Profile
    {
        public HistoricoAnotacaoMappingProfile()
        {
            // Mapeamento de HistoricoAnotacao para HistoricoAnotacaoResponseDto
            CreateMap<HistoricoAnotacao, HistoricoAnotacaoResponseDto>();
        }
    }
}