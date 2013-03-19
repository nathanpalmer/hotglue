using System.Web.Mvc;

namespace Hotglue.Tests.MVCRoutes.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Login(int userId)
        {
            return View();
        }

        public ActionResult Logout()
        {
            return View();
        }

        [NonAction]
        public ActionResult Ignored()
        {
            return View();
        }
    }
}
