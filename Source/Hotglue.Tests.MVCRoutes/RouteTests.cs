using System;
using System.Web.Mvc;
using System.Web.Routing;
using HotGlue;
using HotGlue.Generator;
using HotGlue.Model;
using Hotglue.Tests.MVCRoutes.App_Start;
using Hotglue.Tests.MVCRoutes.Controllers;
using Jurassic;
using NUnit.Framework;
using Shouldly;

namespace Hotglue.Tests.MVCRoutes
{
    [TestFixture]
    public class RouteTests
    {
        public ICompile routeGeneration;
        public String allControllers;
        public String homeController;
        public String authController;

        public UrlHelper Url;

        [TestFixtureSetUp]
        public void Setup()
        {
            RouteCollection routeTable = new RouteCollection();
            RouteConfig.RegisterRoutes(routeTable);
            MVCRouteConfiguration.Initialize(routeTable, typeof(HomeController).Assembly);
            routeGeneration = new MVCRouteGenerator();
            // Generate all routes
            var allReference = new RelativeReference("all.routes", 0)
                {
                    Type = Reference.TypeEnum.Generated
                };
            routeGeneration.Compile(ref allReference);
            allControllers = allReference.Content;
            allControllers.ShouldNotBeEmpty();

            // Generate home routes
            var homeReference = new RelativeReference("home.routes", 0)
            {
                Type = Reference.TypeEnum.Generated
            };
            routeGeneration.Compile(ref homeReference);
            homeController = homeReference.Content;
            homeController.ShouldNotBeEmpty();

            // Generate auth routes
            var authReference = new RelativeReference("auth.routes", 0)
            {
                Type = Reference.TypeEnum.Generated
            };
            routeGeneration.Compile(ref authReference);
            authController = authReference.Content;
            authController.ShouldNotBeEmpty();

            Url = RouteHelper.GetUrlHelper(routeTable);
        }

        [Test]
        public void Evaluate_All_Routes()
        {
            TestHomeRoutes(allControllers);
            TestAuthRoutes(allControllers);
            EvaluateScript(allControllers, "Home.Fake").ShouldBe(missingAction);
            EvaluateScript(allControllers, "Not.Real").ShouldBe(missingController);
        }

        [Test]
        public void Evaluate_Home_Routes()
        {
            TestHomeRoutes(homeController);
            EvaluateScript(homeController, "Auth.Missing").ShouldBe(missingController);
        }

        [Test]
        public void Evaluate_Auth_Routes()
        {
            TestAuthRoutes(authController);
            EvaluateScript(authController, "Home.Missing").ShouldBe(missingController);
        }
        private void TestHomeRoutes(string content)
        {
            EvaluateScript(content, "Home.Index").ShouldBe(Url.Action("Index", "Home"));
            EvaluateScript(content, "Home.Index", "123").ShouldBe(Url.Action("Index", "Home", new { id = 123 }));
            EvaluateScript(content, "Home.Update").ShouldBe(Url.Action("Update", "Home"));
            EvaluateScript(content, "Home.CustomUrl").ShouldBe(Url.Action("CustomUrl", "Home"));
            EvaluateScript(content, "Home.CustomParameter").ShouldBe(Url.Action("CustomParameter", "Home"));
            // Can't generate full custom route time, but can generate partial
            EvaluateScript(content, "Home.CustomParameter", "'bob', 12, 'something'").ShouldStartWith(Url.Action("CustomParameter", "Home"));
        }

        private void TestAuthRoutes(string content)
        {
            EvaluateScript(content, "Auth.Login").ShouldBe(Url.Action("Login", "Auth"));
            EvaluateScript(content, "Auth.Login", "456").ShouldBe(Url.Action("Login", "Auth", new { userId = 456 }));
            EvaluateScript(content, "Auth.Logout").ShouldBe(Url.Action("Logout", "Auth"));
            // Since javascript routes are exposed externally, don't show no action controllers methods.
            EvaluateScript(content, "Auth.Ignored").ShouldBe(missingAction);
        }

        public ScriptEngine engine = new ScriptEngine();
        public const string missingController = "controller not defined";
        public const string missingAction = "action not defined";

        public String scriptTemplate =
@"
(function() {{
var module = {{ exports: {{}} }};
var window = {{}};
{0}
if (!module.exports.{3})
    return '{4}';
if (!module.exports.{1})
    return '{5}';
return module.exports.{1}({2}).toString();
}})();
";

        public String EvaluateScript(string scriptContent, string routeToReturn, string optionalParameters = "")
        {
            var script = String.Format(scriptTemplate, scriptContent, routeToReturn, optionalParameters, routeToReturn.Substring(0, routeToReturn.IndexOf(".")), missingController, missingAction);
            return engine.Evaluate<string>(script);
        }
    }
}
