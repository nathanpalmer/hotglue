using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hotglue.MVCRoutes.Tests.Controllers
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
