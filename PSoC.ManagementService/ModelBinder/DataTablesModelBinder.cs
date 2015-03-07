using System;
using System.Collections.Generic;
using System.Web.Mvc;

using PSoC.ManagementService.Models;

namespace PSoC.ManagementService.ModelBinder
{
    public class DataTablesModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            if (bindingContext.ModelType != typeof(DataTablePageRequestModel))
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName, "Cannot convert value to DataTablePageViewModel");
                return null;
            }

            var pageRequest = new DataTablePageRequestModel
            {
                Draw = GetPageRequestParamValue<Int32>(bindingContext, "Draw", "draw"),
                Start = GetPageRequestParamValue<Int32>(bindingContext, "Start", "start"),
                Length = GetPageRequestParamValue<Int32>(bindingContext, "Length", "length"),
                DistrictFilter = GetPageRequestParamValue<IEnumerable<string>>(bindingContext, "DistrictFilter", "DistrictFilter[]"),
                SchoolFilter = GetPageRequestParamValue<IEnumerable<string>>(bindingContext, "SchoolFilter", "SchoolFilter[]")
            };
            return pageRequest;
        }

        private T GetPageRequestParamValue<T>(ModelBindingContext context, string propertyName, string altPropertyName = "")
        {
            ValueProviderResult valueResult = context.ValueProvider.GetValue(propertyName) ?? context.ValueProvider.GetValue(altPropertyName);
            if (valueResult == null)
                return default(T);
            return (T)valueResult.ConvertTo(typeof(T));
        }
    }
}