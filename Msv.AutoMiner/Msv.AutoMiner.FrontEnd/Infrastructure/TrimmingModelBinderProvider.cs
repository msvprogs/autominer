using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class TrimmingModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Metadata.IsComplexType 
                || context.Metadata.IsCollectionType
                || typeof(IFormFile).IsAssignableFrom(context.Metadata.ModelType))
                return null;

            return new TrimmingModelBinder(
                context.Metadata.Properties.ToDictionary(x => x, context.CreateBinder));
        }
    }
}
