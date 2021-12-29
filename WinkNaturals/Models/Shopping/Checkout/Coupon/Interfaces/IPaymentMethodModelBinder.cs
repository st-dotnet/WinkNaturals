using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces
{
    public interface IPaymentMethod
    {
        bool IsComplete { get; }
        bool IsValid { get; } 
    }

    public class DeviceModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType != typeof(IPaymentMethod))
            {
                return null;
            }

            var subclasses = new[] { typeof(IPaymentMethod), };

            var binders = new Dictionary<Type, (ModelMetadata, IModelBinder)>();
            foreach (var type in subclasses)
            {
                var modelMetadata = context.MetadataProvider.GetMetadataForType(type);
                binders[type] = (modelMetadata, context.CreateBinder(modelMetadata));
            }

            return new DeviceModelBinder(binders);
        }
    }
    public class DeviceModelBinder : IModelBinder
    {
        private Dictionary<Type, (ModelMetadata, IModelBinder)> binders;

        public DeviceModelBinder(Dictionary<Type, (ModelMetadata, IModelBinder)> binders)
        {
            this.binders = binders;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelKindName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, nameof(IPaymentMethod));
            var modelTypeValue = bindingContext.ValueProvider.GetValue(modelKindName).FirstValue;

            IModelBinder modelBinder;
            ModelMetadata modelMetadata;
            if (modelTypeValue == "IPaymentMethod")
            {
                (modelMetadata, modelBinder) = binders[typeof(IPaymentMethod)];
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var newBindingContext = DefaultModelBindingContext.CreateBindingContext(
                bindingContext.ActionContext,
                bindingContext.ValueProvider,
                modelMetadata,
                bindingInfo: null,
                bindingContext.ModelName);

            await modelBinder.BindModelAsync(newBindingContext);
            bindingContext.Result = newBindingContext.Result;

            if (newBindingContext.Result.IsModelSet)
            {
                // Setting the ValidationState ensures properties on derived types are correctly 
                bindingContext.ValidationState[newBindingContext.Result.Model] = new ValidationStateEntry
                {
                    Metadata = modelMetadata,
                };
            }
        }
    }
}