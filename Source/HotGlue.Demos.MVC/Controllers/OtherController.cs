using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotGlue.Demos.MVC.Controllers
{
    public class OtherController : Controller
    {
        //
        // GET: /Other/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Update()
        {
            return View("Index");
        }

    }
}
