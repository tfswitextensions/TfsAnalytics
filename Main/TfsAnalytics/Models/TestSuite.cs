using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TfsAnalytics.Models
{
    public class TestSuite
    {
        public int Id { get; set; }
        public string TestPlanName { get; set; }
        public string TestPlanState { get; set; }
        public string SuitePath { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string LastRunBy { get; set; }
        public string LastRunDate { get; set; }
        public string LastRunOutcome { get; set; }
        public string LastRunOutcomeIcon { get; set; }
        //public Uri TestPlanLink { get; set; }
        //public Uri TestSuiteLink { get; set; }
    }
}