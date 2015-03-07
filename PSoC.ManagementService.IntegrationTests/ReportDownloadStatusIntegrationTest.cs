using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSoC.ManagementService.Data.Helpers;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.IntegrationTests
{
    [TestClass]
    public class ReportDownloadStatusIntegrationTest
    {
        private static HttpClient HttpClientHelper { get; set; }

        private const string ApiReportDownloadStatus = "api/v1/devices/status";

        //private const string ApiEndpoint = "https://localhost"; // For Testing, please don't delete
        private const string ApiEndpoint = "https://pems-dev.cloudapp.net"; 

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up a HTTP client
            HttpClientHelper = new HttpClient();

            // Trust all certificates (helps with self-signed certificates deployed to pems-dev, pems-qa, and pems-load)
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        private async Task Cleanup(Guid deviceId, Guid courseId)
        {
            await DataAccessHelper.ExecuteAsync("DELETE FROM [dbo].[DeviceInstalledCourse] WHERE [DeviceID] = '" + deviceId + "'").ConfigureAwait(false);
            await DataAccessHelper.ExecuteAsync("DELETE FROM [dbo].[Device] WHERE [DeviceID] = '" + deviceId + "'").ConfigureAwait(false);
            await DataAccessHelper.ExecuteAsync("DELETE FROM [dbo].[Course] WHERE [CourseLearningResourceID] = '" + courseId + "'").ConfigureAwait(false);
        }

        // "api/v1/devices/status/{deviceId}"
        [TestMethod]
        public async Task ReportDownloadStatus_ShouldSuccess()
        {
            var deviceId = Guid.NewGuid();
            var courseId1 = Guid.NewGuid();
            var courseId2 = Guid.NewGuid();
            var requestUri = string.Format("{0}/{1}/{2}", ApiEndpoint, ApiReportDownloadStatus, deviceId);
            ExceptionDispatchInfo captureException = null;
            var response = new HttpResponseMessage();

            try
            {
                await
                    DataAccessHelper.ExecuteAsync(string.Format(
                        "INSERT INTO [dbo].[Device] ([DeviceID]) VALUES ('{0}')", deviceId));
                await
                    DataAccessHelper.ExecuteAsync(
                        string.Format("INSERT INTO [dbo].[Course] ([CourseLearningResourceID]) VALUES ('{0}')",
                            courseId1));
                await
                    DataAccessHelper.ExecuteAsync(
                        string.Format(
                            "INSERT INTO [dbo].[DeviceInstalledCourse] ([DeviceID], [CourseLearningResourceID], [PercentDownloaded], [LastUpdated]) VALUES ('{0}', '{1}', {2}, '{3}')",
                            deviceId, courseId1, "3.14", DateTime.UtcNow));

                var courses = new List<Course>()
                {
                    new Course
                    {
                        LearningResourceId = courseId1,
                        Subject = "ELA",
                        Grade = "2",
                        Percent = 1.5m
                    },
                    new Course
                    {
                        LearningResourceId = courseId2,
                        Subject = "MATH",
                        Grade = "3",
                        Percent = 15m
                    }
                };

                using (var content = new ObjectContent<List<Course>>(courses, new JsonMediaTypeFormatter()))
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = content
                    };
                    response = await HttpClientHelper.SendAsync(req).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                captureException = ExceptionDispatchInfo.Capture(ex);
            }

            //Clean
            await Cleanup(deviceId, courseId1).ConfigureAwait(false);
            await DataAccessHelper.ExecuteAsync("DELETE FROM [dbo].[Course] WHERE [CourseLearningResourceID] = '" + courseId2 + "'").ConfigureAwait(false);

            if (captureException != null)
            {
                Assert.Fail(captureException.ToString());
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // "api/v1/devices/status/{deviceId}"
        [TestMethod]
        public async Task ReportDownloadStatus_InvalidDeviceId()
        {
            var courseId1 = Guid.NewGuid();
            var courseId2 = Guid.NewGuid();
            var requestUri = string.Format("{0}/{1}/{2}", ApiEndpoint, ApiReportDownloadStatus, "FAIL_ID");
  
            var courses = new List<Course>()
            {
                new Course
                {
                    LearningResourceId = courseId1,
                    Subject = "ELA",
                    Grade = "2",
                    Percent = 1.5m
                },
                new Course
                {
                    LearningResourceId = courseId2,
                    Subject = "MATH",
                    Grade = "3",
                    Percent = 15m
                }
            };

            using (var content = new ObjectContent<List<Course>>(courses, new JsonMediaTypeFormatter()))
            {
                var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = content
                };
                var response = await HttpClientHelper.SendAsync(req).ConfigureAwait(false);
                Assert.AreEqual(HttpStatusCode.BadRequest,response.StatusCode);
            }
        }
        
        // "api/v1/devices/status/{deviceId}"
        [TestMethod]
        public async Task ReportDownloadStatus_InvalidCourse()
        {
            var deviceId = Guid.NewGuid();
            var requestUri = string.Format("{0}/{1}/{2}", ApiEndpoint, ApiReportDownloadStatus, deviceId);
            using (var content = new ObjectContent<string>(String.Empty, new JsonMediaTypeFormatter()))
            {
                var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = content
                };
                var response = await HttpClientHelper.SendAsync(req).ConfigureAwait(false);
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        // "api/v1/devices/status/{deviceId}"
        [TestMethod]
        public async Task ReportDownloadStatus_ShouldInternalError()
        {
            var deviceId = Guid.NewGuid();
            var courseId1 = Guid.NewGuid();
            var requestUri = string.Format("{0}/{1}/{2}", ApiEndpoint, ApiReportDownloadStatus, deviceId);
            ExceptionDispatchInfo captureException = null;
            var response = new HttpResponseMessage();

            try
            {
                await
                    DataAccessHelper.ExecuteAsync(
                        string.Format("INSERT INTO [dbo].[Course] ([CourseLearningResourceID]) VALUES ('{0}')",
                            courseId1));
               
                var courses = new List<Course>()
                {
                    new Course
                    {
                        LearningResourceId = courseId1,
                        Subject = "ELA",
                        Grade = "2",
                        Percent = 1.5m
                    }
                };

                using (var content = new ObjectContent<List<Course>>(courses, new JsonMediaTypeFormatter()))
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = content
                    };
                    response = await HttpClientHelper.SendAsync(req).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                captureException = ExceptionDispatchInfo.Capture(ex);
            }

            //Clean
            await Cleanup(deviceId, courseId1).ConfigureAwait(false);

            if (captureException != null)
            {
                Assert.Fail(captureException.ToString());
            }

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
