using System;

[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.HotGlueMVCRouteConfig), "PreStart")]

namespace $rootnamespace$.App_Start
{
    public static class HotGlueMVCRouteConfig
    {
        public static void PreStart()
        {
			// This assumes you are using MVC 4 and your routes follows the standard convention. If not, you need to change this or register the routes in global.asax
			// Alternative - HotGlue.Generator.MVCRouteConfiguration.Initialize(RouteTable.Routes, typeof(HotGlueMVCRouteConfig).Assembly);
            HotGlue.Generator.MVCRouteConfiguration.Initialize(RouteConfig.RegisterRoutes, typeof(HotGlueMVCRouteConfig).Assembly);
        }
    }
}