using Microsoft.AspNetCore.Mvc;

namespace Hippo.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public abstract class SuperController : Controller
    {
        public string? Cluster => ControllerContext.RouteData.Values["cluster"] as string;
    }
}