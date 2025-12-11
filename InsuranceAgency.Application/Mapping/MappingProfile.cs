using AutoMapper;
using InsuranceAgency.Application.DTOs;
using InsuranceAgency.Application.DTOs.Client;
using InsuranceAgency.Application.DTOs.Contract;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Contract mappings
            CreateMap<Contract, ContractDto>()
                .ForMember(d => d.Number, opt => opt.MapFrom(s => s.Number ?? string.Empty))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.PremiumAmount, opt => opt.MapFrom(s => s.Premium.Amount))
                .ForMember(d => d.PremiumCurrency, opt => opt.MapFrom(s => s.Premium.Currency))
                .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.StartDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.EndDate.ToDateTime(TimeOnly.MinValue)))
                .ForMember(d => d.AgentId, opt => opt.MapFrom(s => s.AgentId ?? Guid.Empty));

            CreateMap<Contract, ContractDetailsDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Premium, opt => opt.MapFrom(s => s.Premium.Amount))
                .ForMember(d => d.ContractNumber, opt => opt.MapFrom(s => s.Number))
                .ForMember(d => d.ClientFullName, opt => opt.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty))
                .ForMember(d => d.InsuranceServiceTitle, opt => opt.MapFrom(s => s.Service != null ? s.Service.Name : string.Empty));

            CreateMap<Contract, ContractSummaryDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Premium, opt => opt.MapFrom(s => s.Premium.Amount))
                .ForMember(d => d.ContractNumber, opt => opt.MapFrom(s => s.Number));

            // Client mappings
            CreateMap<Client, ClientDto>();
        }
    }
}
