using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces
{
   // public class IPaymentMethodModelBinder : IModelBinder
   // {
        //public Task BindModelAsync(ModelBindingContext bindingContext)
        //{
        //    if (modelType.Name != "IPaymentMethod") return base.CreateModel(controllerContext, bindingContext, modelType);

        //    var paymentMethodType = bindingContext.ModelName + ".PaymentMethodType";

        //    var rawClassName = bindingContext.ValueProvider.GetValue(paymentMethodType);
        //    if (rawClassName == null || string.IsNullOrEmpty(rawClassName.ToString())) throw new Exception("You cannot model-bind to a property of type {0} without passing the desired model class name through a form field named '{1}'.".FormatWith(modelType.ToString(), paymentMethodType));

        //    var className = rawClassName.AttemptedValue.ToString();
        //    modelType = Type.GetType(className);

        //    if (modelType.Name == "String[]") throw new Exception("You cannot pass more than one form field named '{0}' when model-binding to IPaymentMethod.".FormatWith(paymentMethodType));

        //    if (modelType != null)
        //    {
        //        var instance = Activator.CreateInstance(modelType);
        //        bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => instance, modelType);

        //        return instance;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //protected override object CreateModel(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext, Type modelType)
        //{
        //    if (modelType.Name != "IPaymentMethod") return base.CreateModel(controllerContext, bindingContext, modelType);

        //    var paymentMethodType = bindingContext.ModelName + ".PaymentMethodType";

        //    var rawClassName = bindingContext.ValueProvider.GetValue(paymentMethodType);
        //    if (rawClassName == null || string.IsNullOrEmpty(rawClassName.ToString())) throw new Exception("You cannot model-bind to a property of type {0} without passing the desired model class name through a form field named '{1}'.".FormatWith(modelType.ToString(), paymentMethodType));

        //    var className = rawClassName.AttemptedValue.ToString();
        //    modelType = Type.GetType(className);

        //    if (modelType.Name == "String[]") throw new Exception("You cannot pass more than one form field named '{0}' when model-binding to IPaymentMethod.".FormatWith(paymentMethodType));

        //    if (modelType != null)
        //    {
        //        var instance = Activator.CreateInstance(modelType);
        //        bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => instance, modelType);

        //        return instance;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    
}
