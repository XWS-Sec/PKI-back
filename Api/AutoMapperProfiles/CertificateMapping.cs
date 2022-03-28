using System.Collections.Generic;
using System.Linq;
using Api.DTO;
using AutoMapper;
using Model.Certificates;
using Model.Users;

namespace Api.AutoMapperProfiles
{
    public class CertificateMapping : Profile
    {
        public CertificateMapping()
        {
            CreateMap<NewCertificateDto, Certificate>();
            CreateMap<Certificate, CertificateDto>();
            CreateMap<User, List<CertificateDto>>()
                .BeforeMap<UserCertificateDtoAction>();
        }
    }
    
    public class UserCertificateDtoAction : IMappingAction<User, List<CertificateDto>>
    {
        public void Process(User source, List<CertificateDto> destination, ResolutionContext context)
        {
            destination.AddRange(source.Certificates.Select(x =>
            {
                var curr = context.Mapper.Map<CertificateDto>(x);
                curr.OwnerName = source.Name;
                curr.OwnerSurname = source.Surname;

                return curr;
            }));
        }
    }
}