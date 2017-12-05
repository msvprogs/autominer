using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class TrimmingModelBinder : ComplexTypeModelBinder
    {
        public TrimmingModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders) 
            : base(propertyBinders)
        { }

        protected override void SetProperty(
            ModelBindingContext bindingContext, string modelName, ModelMetadata propertyMetadata, ModelBindingResult result)
        {
            if (result.Model is string srcString)
                result = ModelBindingResult.Success(srcString.Trim());

            base.SetProperty(bindingContext, modelName, propertyMetadata, result);
        }
    }
}
