using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model.Certificates;
using Model.Constants;
using Model.Users;
using Services.Certificates;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly GetCertificateService _getCertificateService;
        private readonly IMapper _mapper;
        private readonly CertificateIssuingService _certificateIssuingService;

        public CertificatesController(UserManager<User> userManager, GetCertificateService getCertificateService, IMapper mapper,
            CertificateIssuingService certificateIssuingService)
        {
            _userManager = userManager;
            _getCertificateService = getCertificateService;
            _mapper = mapper;
            _certificateIssuingService = certificateIssuingService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = await _getCertificateService.GetUserCertificates(_userManager.GetUserName(User));
            return response == null 
                ? BadRequest("User not found")
                : Ok(_mapper.Map<List<CertificateDto>>(response));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Intermediate")]
        public async Task<IActionResult> Post(NewCertificateDto newCertificate)
        {
            try
            {
                var userName = _userManager.GetUserName(User);

                var certificate = await _certificateIssuingService.CreateCertificate(
                    _userManager.GetUserName(User),
                    newCertificate.UserOwner,
                    newCertificate.KeyUsageFlagsCommaSeparated,
                    newCertificate.ValidFrom,
                    newCertificate.ValidTo,
                    newCertificate.Subject,
                    newCertificate.IssuerSerialNumber);
                
                return Ok(certificate.SerialNumber);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}