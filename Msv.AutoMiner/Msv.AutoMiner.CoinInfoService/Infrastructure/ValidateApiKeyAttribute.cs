using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.CoinInfoService.Infrastructure
{
    public class ValidateApiKeyAttribute : TypeFilterAttribute
    {
        public ValidateApiKeyAttribute(ApiKeyType type, bool decreaseCounter = true)
            : base(typeof(ValidateApiKeyFilter))
        {
            Arguments = new object[] {type, decreaseCounter};
        }

        private class ValidateApiKeyFilter : IAsyncActionFilter
        {
            private readonly IValidateApiKeyFilterStorage m_Storage;
            private readonly ApiKeyType m_Type;
            private readonly bool m_DecreaseCounter;

            public ValidateApiKeyFilter(IValidateApiKeyFilterStorage storage, ApiKeyType type, bool decreaseCounter)
            {
                m_Storage = storage;
                m_Type = type;
                m_DecreaseCounter = decreaseCounter;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var apiKeyString = context.RouteData.Values["apikey"] as string;
                if (string.IsNullOrEmpty(apiKeyString))
                {
                    context.Result = new ForbidResult();
                    return;
                }
                var apiKey = await m_Storage.GetApiKey(apiKeyString);
                if (apiKey == null
                    || apiKey.Type != m_Type
                    || apiKey.UsagesLeft <= 0
                    || apiKey.Expires < DateTime.UtcNow)
                {
                    context.Result = new ForbidResult();
                    return;
                }
                if (apiKey.UsagesLeft != null && m_DecreaseCounter)
                {
                    apiKey.UsagesLeft--;
                    await m_Storage.SaveApiKey(apiKey);
                }
                await next();
            }
        }
    }
}
