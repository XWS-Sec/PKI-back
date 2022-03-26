using System;
using Model.Users;

namespace Model.Certificates
{
    public class Certificate
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Issuer { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Subject { get; set; }

        public CertificateStatus Status { get; set; }
        
        public string UserId { get; set; }
    }
}