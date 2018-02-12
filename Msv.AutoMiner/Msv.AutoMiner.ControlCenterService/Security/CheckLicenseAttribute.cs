using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Msv.AutoMiner.Common.Licensing;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.ControlCenterService.Security
{
    public class CheckLicenseAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
#if DEBUG
                return;
#endif

            var model = context.ActionArguments.Values
                .OfType<LicensedRequestBase>()
                .FirstOrDefault();
            if (model == null)
                throw new InvalidOperationException("Action model doesn't support license checking");

            if (LicenseData.Current.LicenseId != model.LicenseId)
                context.Result = new UnauthorizedResult();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        { }
    }
}
