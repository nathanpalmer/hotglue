using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HotGlue.Demos.MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CustomUrl",
                url: "Awesome",
                defaults: new { controller = "Home", action = "CustomUrl" },
                constraints: new { controller = "Home", action = "CustomUrl" }
            );

            routes.MapRoute(
                name: "CustomUrlLong",
                url: "Customer/{age}/{name}",
                defaults: new { controller = "Home", action = "CustomParameter" },
                constraints: new { controller = "Home", action = "CustomParameter" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "Logout",
                defaults: new { controller = "Auth", action = "Logout" },
                constraints: new { controller = "Auth", action = "Logout" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}