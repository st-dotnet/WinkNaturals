using Microsoft.AspNetCore.Mvc;
using WinkNaturals.Models;

namespace WinkNaturals.Controllers
{
    // [Authorize]
    public class BaseController : ControllerBase
    {
        protected CustomerCreateModel Identity => Request.HttpContext.Items["Customer"] as CustomerCreateModel;
    }
}
