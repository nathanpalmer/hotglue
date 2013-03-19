using System.Web.Mvc;

namespace Hotglue.Tests.MVCRoutes.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int id)
        {
            return View();
        }

        public ActionResult Update()
        {
            return View();
        }

        public ActionResult CustomUrl()
        {
            return View();
        }

        public ActionResult CustomParameter(string name, int age, string extraParam)
        {
            return View();
        }
    }
}
