using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly ExportCertificateService _exportCertificateService;
        private readonly CertificateRevocationService _certificateRevocationService;

        public CertificatesController(UserManager<User> userManager, GetCertificateService getCertificateService, IMapper mapper,
            CertificateIssuingService certificateIssuingService, ExportCertificateService exportCertificateService, CertificateRevocationService certificateRevocationService)
        {
            _userManager = userManager;
            _getCertificateService = getCertificateService;
            _mapper = mapper;
            _certificateIssuingService = certificateIssuingService;
            _exportCertificateService = exportCertificateService;
            _certificateRevocationService = certificateRevocationService;
        }

        [HttpGet]
        [Authorize]
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

        [HttpGet("{serialNumber}")]
        [Authorize]
        public async Task<IActionResult> Export([Required] string serialNumber)
        {
            try
            {
                var username = _userManager.GetUserName(User);

                var cert = await _exportCertificateService.Export(username, serialNumber);

                var lastSlash = cert.LastIndexOf('\\');
                var fileName = cert.Substring(lastSlash + 1);

                var bytes = System.IO.File.ReadAllBytes(cert);
                return File(bytes, "application/octet-stream", fileName);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("revoke/{serialNumber}")]
        [Authorize]
        public async Task<IActionResult> Revoke([Required] string serialNumber, [Required] string revocationReason)
        {
            try
            {
                var username = _userManager.GetUserName(User);

                await _certificateRevocationService.Revoke(username, serialNumber, revocationReason);

                return Ok($"Certificate with serial number {serialNumber} and all linked with it are revoked.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}