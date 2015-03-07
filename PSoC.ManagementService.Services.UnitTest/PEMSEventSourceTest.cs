using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.UnitTest
{
    [TestClass]
    public class PEMSEventSourceTest
    {
        private IPEMSEventSource eventSource;
        private Mock<ILogger> _mockLogger;
        private const String SafeValue = "SAFE";
        private const String SecureValue = "SECURE";
        private static readonly String SampleJsonArray = CreateSampleJson(true);
        private static readonly String SampleJsonObject = CreateSampleJson();

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger>();
            eventSource = PEMSEventSource.Log;

            // Arrange: Inject logger to event source
            eventSource.Logger = _mockLogger.Object; 
        }

        [TestMethod]
        public void PEMSEventSourceTest_WriteLog_VerifyLogRequest()
        {
            // Arrange
            var logRequest = new LogRequest {Logger = "Unit Test", Level = LogLevel.Info};
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns((LogLevel l) => l != LogLevel.Off);
            _mockLogger.Setup(x => x.Log(It.IsAny<LogRequest>()));

            // Act
            eventSource.WriteLog(logRequest);

            // Assert
            _mockLogger.VerifyAll();
        }

        [TestMethod]
        public void PEMSEventSourceTest_WriteLog_FilterJsonArray_VerifyLogRequest()
        {
            // Arrange
            var logRequestArray = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonArray, JsonResponse = SampleJsonArray };
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns((LogLevel l) => l != LogLevel.Off);

            // Act
            eventSource.WriteLog(logRequestArray);

            // Assert
            Assert.AreNotEqual(SampleJsonArray, logRequestArray.JsonRequest);
            Assert.AreNotEqual(SampleJsonArray, logRequestArray.JsonResponse);
        }

        [TestMethod]
        public void PEMSEventSourceTest_WriteLog_FilterJsonObject_VerifyLogRequest()
        {
            // Arrange
            var logRequestObject = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonObject, JsonResponse = SampleJsonObject };
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns((LogLevel l) => l != LogLevel.Off);

            // Act
            eventSource.WriteLog(logRequestObject);

            // Assert
            Assert.AreNotEqual(SampleJsonObject, logRequestObject.JsonRequest);
            Assert.AreNotEqual(SampleJsonObject, logRequestObject.JsonResponse);
        }

        [TestMethod]
        public void PEMSEventSourceTest_PingLog_VerifyLogRequest()
        {
            // Arrange
            var numLevels = Enum.GetNames(typeof(LogLevel)).Length-1; // All available log levels except Off
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns((LogLevel l) => l != LogLevel.Off);
            _mockLogger.Setup(x => x.Log(It.IsAny<LogRequest>()));

            // Act
            eventSource.PingLog();

            // Assert
            _mockLogger.VerifyAll();
            _mockLogger.Verify(x => x.Log(It.IsAny<LogRequest>()), Times.Exactly(numLevels)); 
        }


        [TestMethod]
        public void PEMSEventSourceTest_AppendDeviceLicenseRequest_ExpectNull()
        {
            // Act
            var actual = eventSource.Append(null, null);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void PEMSEventSourceTest_AppendDeviceLicenseRequest_ExpectNotNull()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var deviceReq = new DeviceLicenseRequest { DeviceId = deviceId };

            // Act
            var actual = eventSource.Append(deviceReq, null);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(deviceId, actual.DeviceId);
        }

        [TestMethod]
        public void PEMSEventSourceTest_AppendDeviceLicenseRequest_ExpectUpdatedLogRequest()
        {
            // Arrange
            var deviceId = Guid.NewGuid().ToString();
            var deviceReq = new DeviceLicenseRequest { DeviceId = deviceId };
            var logRequest = new LogRequest();

            // Act
            var actual = eventSource.Append(deviceReq, logRequest);

            // Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(deviceId, actual.DeviceId);
        }

        [TestMethod]
        public void PEMSEventSourceTest_Filter_JsonArrayByNewtonsoft_VerifyLogRequest()
        {
            // Arrange
            var logRequest = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonArray, JsonResponse = SampleJsonArray };

            // Act
            Debug.WriteLine("Before: " + logRequest.JsonRequest);
            var filtered = eventSource.Filter(logRequest, PEMSEventSource.JsonFilterMethod.Newtonsoft);
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("After: " + filtered.JsonRequest);

            // Assert
            Assert.IsTrue(filtered.JsonRequest.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonRequest.Contains(SecureValue));
            Assert.IsTrue(filtered.JsonResponse.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonResponse.Contains(SecureValue));
        }

        [TestMethod]
        public void PEMSEventSourceTest_Filter_JsonArrayByRegex_VerifyLogRequest()
        {
            // Arrange
            var logRequest = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonArray, JsonResponse = SampleJsonArray };
            
            // Act
            Debug.WriteLine("Before: " + logRequest.JsonRequest);
            var filtered = eventSource.Filter(logRequest, PEMSEventSource.JsonFilterMethod.Regex);
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("After: " + filtered.JsonRequest);

            // Assert
            Assert.IsTrue(filtered.JsonRequest.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonRequest.Contains(SecureValue));
            Assert.IsTrue(filtered.JsonResponse.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonResponse.Contains(SecureValue));
        }

        [TestMethod]
        public void PEMSEventSourceTest_Filter_JsonObjectByNewtonsoft_VerifyLogRequest()
        {
            // Arrange
            var logRequest = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonObject, JsonResponse = SampleJsonObject };

            // Act
            Debug.WriteLine("Before: " + logRequest.JsonRequest);
            var filtered = eventSource.Filter(logRequest, PEMSEventSource.JsonFilterMethod.Newtonsoft);
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("After: " + filtered.JsonRequest);

            // Assert
            Assert.IsTrue(filtered.JsonRequest.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonRequest.Contains(SecureValue));
            Assert.IsTrue(filtered.JsonResponse.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonResponse.Contains(SecureValue));
        }

        [TestMethod]
        public void PEMSEventSourceTest_Filter_JsonObjectByRegex_VerifyLogRequest()
        {
            // Arrange
            var logRequest = new LogRequest { Logger = "Unit Test", Level = LogLevel.Info, JsonRequest = SampleJsonObject, JsonResponse = SampleJsonObject };

            // Act
            Debug.WriteLine("Before: " + logRequest.JsonRequest);
            var filtered = eventSource.Filter(logRequest, PEMSEventSource.JsonFilterMethod.Regex);
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("After: " + filtered.JsonRequest);

            // Assert
            Assert.IsTrue(filtered.JsonRequest.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonRequest.Contains(SecureValue));
            Assert.IsTrue(filtered.JsonResponse.Contains(SafeValue));
            Assert.IsTrue(!filtered.JsonResponse.Contains(SecureValue));
        }

        /// <summary>
        /// Create sample JSON
        /// </summary>
        /// <param name="isArray"></param>
        /// <returns></returns>
        private static String CreateSampleJson(Boolean isArray = false)
        {
            var sb = new StringBuilder();
            
            sb.Append(" { ");
            sb.Append(@" ""Safe:"" : ");
            sb.Append(@"""");
            sb.Append(SafeValue);
            sb.Append(@"""");
            sb.Append(" , ");
            var fields = PEMSEventSource.FilteredFields;
            foreach (var f in fields)
            {
                sb.Append(@"""");
                sb.Append(f);
                sb.Append(@"""");
                sb.Append(" : ");
                sb.Append(@"""");
                sb.Append(SecureValue);
                sb.Append(@"""");
                sb.Append(" , ");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(" } ");

            var result = sb.ToString();
            if (isArray)
                return " [ " + result + ", " + result + " ] ";

            return result;
        }
    }
}
