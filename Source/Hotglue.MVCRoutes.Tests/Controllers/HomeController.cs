using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hotglue.MVCRoutes.Tests.Controllers
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
