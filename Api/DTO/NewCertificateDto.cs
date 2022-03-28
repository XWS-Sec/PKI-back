using System;
using System.ComponentModel.DataAnnotations;

namespace Api.DTO
{
    public class NewCertificateDto
    {
        //If null the signer is itself
        public string? IssuerSerialNumber { get; set; }
       
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime ValidFrom { get; set; }
        
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime ValidTo { get; set; }
        
        //if null the owner is the current user
        public string? UserOwner { get; set; }
        
        //if here is found X509KeyUsageFlags.KeyCertSign then it is used for signing others (so it is CA)
        public string KeyUsageFlagsCommaSeparated { get; set; }
    }
}