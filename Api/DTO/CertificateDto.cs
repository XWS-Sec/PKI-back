using System;

namespace Api.DTO
{
    public class CertificateDto
    {
        public string SerialNumber { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Issuer { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }

        public string OwnerName { get; set; }
        public string OwnerSurname { get; set; }
        public string OwnerRole { get; set; }
    }
}