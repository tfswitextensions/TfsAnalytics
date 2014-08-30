using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TfsAnalytics.Models;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using System.Configuration;

namespace TfsAnalytics.Services
{
    public class TcmService
    {
        ITestManagementService tcm = null;

        public TcmService()
        {
            var tfsCollectionUri = ConfigurationManager.AppSettings["TfsCollectionUri"];
            var tpc = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsCollectionUri));
            this.tcm = (ITestManagementService)tpc.GetService(typeof(ITestManagementService));
        }

        public List<TestResult> GetTestResults(string teamProjectName, int testCaseId)
        {
            var project = this.tcm.GetTeamProject(teamProjectName);
            return ListResultsForTestCase(project, testCaseId);
        }

        private List<TestResult> ListResultsForTestCase(ITestManagementTeamProject project, int testCaseId)
        {
            var testResults = new List<TestResult>();
            foreach (var result in project.TestResults.ByTestId(testCaseId))
            {
                var tr = result.GetTestRun();
                if (tr != null)
                {
                    var testResult = new TestResult()
                    {
                        Id = result.Id.TestRunId,
                        State = result.State.ToString(),
                        Outcome = result.Outcome.ToString(),
                        OutcomeIcon = GetOutcomeIcon(result.Outcome),
                        Title = tr.Title,
                        RunBy = result.RunByName,
                        RunDate = result.DateStarted.ToString("yyyy-MM-dd HH:mm:ss"),
                        RunDuration = result.Duration,
                        Machine = result.ComputerName,
                        RunState = tr.State.ToString(),
                        Environment = tr.TestEnvironmentId.ToString(),
                        Controller = tr.Controller,
                        TestSettings = (tr.TestSettings != null) ? tr.TestSettings.Name : string.Empty,
                        TestConfiguration = result.TestConfigurationName
                    };

                    testResults.Add(testResult);
                }
            }

            return testResults.OrderByDescending(x => x.RunDate).ToList();
        }

        private string GetOutcomeIcon(TestOutcome outcome)
        {
            const string IconSource = "~/img/Status_{0}_16x16.png";
            var outcomeIcon = "";
            switch (outcome)
            {
                case TestOutcome.Passed:
                    outcomeIcon =  string.Format(IconSource, "Ok");
                    break;
                case TestOutcome.Failed:
                    outcomeIcon =  string.Format(IconSource, "Error");
                    break;
                case TestOutcome.Inconclusive:
                    outcomeIcon =  string.Format(IconSource, "Alert");
                    break;
                case TestOutcome.Paused:
                    outcomeIcon = string.Format(IconSource, "Pause");
                    break;
                case TestOutcome.Blocked:
                    outcomeIcon = string.Format(IconSource, "Blocked");
                    break;
                case TestOutcome.NotApplicable:
                    outcomeIcon = string.Format(IconSource, "NotApplicable");
                    break;                    
                default:
                    outcomeIcon = string.Format(IconSource, "Info");
                    break;
            }
            return outcomeIcon;
        }

        public List<TestSuite> GetTestSuites2(string teamProjectName, int testCaseId)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var project = this.tcm.GetTeamProject(teamProjectName);

            var testSuites = new List<TestSuite>();

            foreach (ITestPlan testPlan in project.TestPlans.Query("SELECT * from TestPlan"))
            {
                var points = testPlan.QueryTestPoints(string.Format("SELECT * FROM TestPoint where TestCaseId = {0}", testCaseId));
                if (points.Count > 0)
                {
                    var suiteIds = points.Select((point) => point.SuiteId).Distinct();
                    //PrintSuites(suiteIds, testPlan.Id);
                //    points[0].gets
                //testSuites.Add(new TestSuite()
                //    {
                //        Id = testSuite.Id,
                //        TestPlanName = testSuite.Plan.Name,
                //        TestPlanState = testSuite.Plan.State.ToString(),
                //        SuitePath = parentPath,
                //        Title = testSuite.Title,
                //        State = testSuite.State.ToString(),
                //        LastRunBy = (result != null) ? result.OwnerName : string.Empty,
                //        LastRunDate = (result != null) ? result.DateStarted.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                //        LastRunOutcome = (result != null) ? result.Outcome.ToString() : "Not run",
                //        LastRunOutcomeIcon = (result != null) ? GetOutcomeIcon(result.Outcome) : GetOutcomeIcon(TestOutcome.NotExecuted)
                //    });

                }
            }

            var testPlans = project.TestPlans.Query("SELECT * from TestPlan").ToList();
            foreach (ITestPlan testPlan in testPlans)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Processing plan {0}.", testPlan.Name));

                var allTestCases = testPlan.RootSuite.AllTestCases.ToArray();
                if (allTestCases.Count(tc => tc.Id == testCaseId) > 0)
                {
                    testSuites.AddRange(FindInSuites(testPlan.Name, testPlan.RootSuite, testCaseId));
                }
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("---Run completed. Time spent: {0}", sw.Elapsed.ToString()));

            return testSuites.OrderBy(x => x.SuitePath).ToList();
        }

        public List<TestSuite> GetTestSuites(string teamProjectName, int testCaseId)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var project = this.tcm.GetTeamProject(teamProjectName);

            var testSuites = new List<TestSuite>();

            var testPlans = project.TestPlans.Query("SELECT * from TestPlan").ToList();
            foreach (ITestPlan testPlan in testPlans)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Processing plan {0}.", testPlan.Name));

                var allTestCases = testPlan.RootSuite.AllTestCases.ToArray();
                if (allTestCases.Count(tc => tc.Id == testCaseId) > 0)
                {
                    testSuites.AddRange(FindInSuites(testPlan.Name, testPlan.RootSuite, testCaseId));
                }
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("---Run completed. Time spent: {0}", sw.Elapsed.ToString()));

            return testSuites.OrderBy(x => x.SuitePath).ToList();
        }

        private List<TestSuite> FindInSuites(string parentPath, IStaticTestSuite testSuite, int testCaseId)
        {
            var testSuites = new List<TestSuite>();

            foreach (ITestSuiteEntry entry in testSuite.Entries)
            {
                if (entry.EntryType == TestSuiteEntryType.TestCase)
                {
                    if (entry.Id == testCaseId)
                    {
                        ITestCaseResult result = null;
                        var point = testSuite.Plan.QueryTestPoints(string.Format("SELECT * FROM TestPoint WHERE SuiteId = {0} AND TestCaseId = {1}", testSuite.Id, testCaseId)).FirstOrDefault();
                        if (point != null)
                        {
                            result = point.MostRecentResult;
                        }

                        testSuites.Add(new TestSuite() { 
                            Id = testSuite.Id, 
                            TestPlanName = testSuite.Plan.Name,
                            TestPlanState = testSuite.Plan.State.ToString(),
                            SuitePath = parentPath,
                            Title = testSuite.Title, 
                            State = testSuite.State.ToString(),
                            LastRunBy = (result != null) ? result.OwnerName : string.Empty,
                            LastRunDate = (result != null) ? result.DateStarted.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                            LastRunOutcome = (result != null) ? result.Outcome.ToString() : "Not run",
                            LastRunOutcomeIcon = (result != null) ? GetOutcomeIcon(result.Outcome) : GetOutcomeIcon(TestOutcome.NotExecuted)
                             });
                    }
                }
                else
                {
                    IStaticTestSuite staticSuite = entry.TestObject as IStaticTestSuite;

                    if (staticSuite != null)
                    {
                        testSuites.AddRange(FindInSuites(string.Format("{0}/{1}", parentPath, staticSuite.Title), staticSuite, testCaseId));
                    }
                    else
                    {
                        IDynamicTestSuiteBase dynamicSuite = entry.TestObject as IDynamicTestSuiteBase;
                        if (dynamicSuite != null)
                        {
                            if (dynamicSuite.TestCases.Where(tc => tc.Id == testCaseId).Count() > 0)
                            {
                                ITestCaseResult result = null;
                                var point = dynamicSuite.Plan.QueryTestPoints(string.Format("SELECT * FROM TestPoint WHERE SuiteId = {0} AND TestCaseId = {1}", dynamicSuite.Id, testCaseId)).FirstOrDefault();
                                if (point != null)
                                {
                                    result = point.MostRecentResult;
                                }

                                testSuites.Add(new TestSuite()
                                {
                                    Id = dynamicSuite.Id,
                                    TestPlanName = dynamicSuite.Plan.Name,
                                    TestPlanState = dynamicSuite.Plan.State.ToString(),
                                    SuitePath = string.Format("{0}/{1}", parentPath, dynamicSuite.Title),
                                    Title = dynamicSuite.Title,
                                    State = dynamicSuite.State.ToString(),
                                    LastRunBy = (result != null) ? result.OwnerName : string.Empty,
                                    LastRunDate = (result != null) ? result.DateStarted.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty,
                                    LastRunOutcome = (result != null) ? result.Outcome.ToString() : "Not run",
                                    LastRunOutcomeIcon = (result != null) ? GetOutcomeIcon(result.Outcome) : GetOutcomeIcon(TestOutcome.NotExecuted)
                                });
                            }
                        }
                    }
                }
            }

            return testSuites;
        }



    }
}