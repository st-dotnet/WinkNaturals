using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using WinkNaturals.Models;
using IAuthorizationFilter = Microsoft.AspNetCore.Mvc.Filters.IAuthorizationFilter;

namespace WinkNaturals.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (CustomerCreateModel)context.HttpContext.Items["User"];
            if (user == null)
            {
                // not logged in
                context.Result = new Microsoft.AspNetCore.Mvc.JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
