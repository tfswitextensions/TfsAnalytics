using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TfsAnalytics.Services;

namespace TfsAnalytics.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Results(string teamProjectName, int testCaseId)
        {
            var svc = new TcmService();
            var results = svc.GetTestResults(teamProjectName, testCaseId);
            return View(results);
        }

        public ActionResult Suites(string teamProjectName, int testCaseId)
        {
            var svc = new TcmService();
            var suites = svc.GetTestSuites(teamProjectName, testCaseId);
            return View(suites);
        }
    }
}