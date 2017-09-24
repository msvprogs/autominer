using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using NLog;

namespace Msv.AutoMiner.ControlCenterService.Security
{
    public class AuthenticateRigByCertificateAttribute : TypeFilterAttribute
    {
        public AuthenticateRigByCertificateAttribute()
            : base(typeof(AuthenticateRigByCertificateFilter))
        { }

        private class AuthenticateRigByCertificateFilter : IAsyncActionFilter
        {
            private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

            private readonly ICertificateService m_CertificateService;

            public AuthenticateRigByCertificateFilter(ICertificateService certificateService)
                => m_CertificateService = certificateService;

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var ip = context.HttpContext.Connection.RemoteIpAddress;
                M_Logger.Info($"Starting authentication, remote IP {ip}");
                var certificate = context.HttpContext.Connection.ClientCertificate;
                if (certificate == null)
                {
                    M_Logger.Warn($"{ip}: Client certificate not found");
                    context.Result = new ForbidResult();
                    return;
                }
                //if (!certificate.Verify())
                //{
                //    M_Logger.Warn($"{ip}: Client certificate is invalid");
                //    context.Result = new ForbidResult();
                //    return;
                //}
                var rig = await m_CertificateService.AuthenticateRig(certificate);
                if (rig == null)
                {
                    M_Logger.Warn($"{ip}: Rig with the specified CN and serial not found ({certificate.SubjectName}, serial {certificate.SerialNumber})");
                    context.Result = new ForbidResult();
                    return;
                }
                if (!rig.IsActive)
                {
                    M_Logger.Warn($"{ip}: Rig {rig.Id} ({rig.Name}) is inactive");
                    context.Result = new ForbidResult();
                    return;
                }
                M_Logger.Info($"{ip}: Authenticated rig {rig.Id} ({rig.Name})");
                context.RouteData.Values["rigId"] = rig.Id;
                await next.Invoke();
            }
        }
    }
}
