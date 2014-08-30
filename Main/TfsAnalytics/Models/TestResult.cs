using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TfsAnalytics.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string State { get; set; }
        public string Outcome { get; set; }
        public string OutcomeIcon { get; set; }
        public string Title { get; set; }
        public string RunBy { get; set; }
        public string RunDate { get; set; }
        public TimeSpan RunDuration { get; set; }
        public string Machine { get; set; }
        //public string RunType { get; set; }
        public string RunState { get; set; }
        public string Environment { get; set; }
        public string Controller { get; set; }
        public string TestSettings { get; set; }
        public string TestConfiguration { get; set; }
    }
}