using Microsoft.EntityFrameworkCore;
using Model.Shared;

namespace Model.Certificates.Repository
{
    public class CertificateRepository : BaseRepository<Certificate, int>, ICertificateRepository
    {
        public CertificateRepository(AppDbContext context) : base(context)
        {
        }
    }
}