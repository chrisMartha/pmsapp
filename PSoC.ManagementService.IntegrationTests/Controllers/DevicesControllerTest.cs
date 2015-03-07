using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSoC.ManagementService.IntegrationTests.Helpers;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.IntegrationTests.Controllers
{
    [TestClass]
    public class DevicesControllerTest
    {
        private const string ApiDevEndpoint = "https://pems-dev.cloudapp.net";
        private const string ApiLoadEndpoint = "https://pems-load.cloudapp.net";
        private const string ApiPut = "api/v1/devices";

        private const bool _runmultiThreadedLoadTest = false;
        private const bool _verbose = false;
        private readonly Tuple<int, int>[] _loadMultiThreadedTestConfigurations =
        {
            new Tuple<int, int>(1, 10000),
            new Tuple<int, int>(2, 5000),
            new Tuple<int, int>(4, 2500),
            new Tuple<int, int>(5, 2000),
            new Tuple<int, int>(8, 1250),
            new Tuple<int, int>(10, 1000),
            new Tuple<int, int>(20, 500),
            new Tuple<int, int>(25, 400),
            new Tuple<int, int>(40, 250),
            new Tuple<int, int>(50, 200),
            new Tuple<int, int>(80, 125),
            new Tuple<int, int>(100, 100),
            new Tuple<int, int>(125, 80),
            new Tuple<int, int>(200, 50),
            new Tuple<int, int>(250, 40),
            new Tuple<int, int>(400, 25),
            new Tuple<int, int>(500, 20),
            new Tuple<int, int>(1000, 10)
        };
        private volatile int _loadTestCount;
        private long _totalElapsedMs;

        [TestInitialize]
        public void Initialize()
        {
            // Trust all certificates (helps with self-signed certificates deployed to pems-dev, pems-qa, and pems-load)
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        [TestMethod]
        public async Task DevicesController_Put_QuickExternalLoadTest()
        {
            // Use the same AP BSSID to produce a mix of both - licenses and rejects
            string wifiBssid = DataGenerator.GetRandomMacAddress();

            const int quickLoadTestCount = 10; //100;
            Stopwatch sw = new Stopwatch();
            long totalElapsedMs = 0L;

            for (int i = 0; i < quickLoadTestCount; i++)
            {
                Guid deviceId = Guid.NewGuid();
                DeviceLicenseRequest deviceLicenseRequest = CreateDeviceLicenseRequest(deviceId, wifiBssid);
                using (var content = new ObjectContent<DeviceLicenseRequest>(deviceLicenseRequest, new JsonMediaTypeFormatter()))
                {
                    string requestUri = string.Format("{0}/{1}/{2}", ApiDevEndpoint, ApiPut, deviceId);
                    var req = new HttpRequestMessage(HttpMethod.Put, requestUri)
                    {
                        Content = content
                    };
                    var client = new HttpClient();

                    sw.Start();

                    HttpResponseMessage response = await client.SendAsync(req).ConfigureAwait(false);
                    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                    sw.Stop();

                    long elapsedMs = sw.ElapsedMilliseconds;
                    totalElapsedMs += elapsedMs;
                    Debug.WriteLine("DevicesController Put load test #{0} took {1}ms", i, elapsedMs);

                    sw.Reset();
                }
            }

            long avgMs = totalElapsedMs / quickLoadTestCount;
            Debug.WriteLine("DevicesController Put {0} load tests took average {1}ms", quickLoadTestCount, avgMs);
            Assert.IsTrue(avgMs < 1000L);
        }

        [TestMethod]
        public void DevicesController_Put_MultithreadedExternalLoadTest()
        {
            if (_runmultiThreadedLoadTest)
            {
                foreach (Tuple<int, int> loadTestConfiguration in _loadMultiThreadedTestConfigurations)
                {
                    int threadCount = loadTestConfiguration.Item1;
                    _loadTestCount = loadTestConfiguration.Item2;
                    Thread[] threads = new Thread[threadCount];

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i] = new Thread(LoadTest);
                    }

                    Stopwatch sw = new Stopwatch();

                    sw.Start();

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i].Start(i.ToString());
                    }

                    foreach (Thread thread in threads)
                    {
                        thread.Join();
                    }

                    sw.Stop();
                    long totalElapsedMs = sw.ElapsedMilliseconds;

                    Debug.WriteLine("DevicesController Put {0} threads {1} tests took average {2}ms total {3}ms", threadCount, _loadTestCount, _totalElapsedMs / (threadCount * _loadTestCount), totalElapsedMs);

                    // Reset total individual times for another round...
                    _totalElapsedMs = 0L;
                }
            }
        }

        private void LoadTest(object threadNum)
        {
            Stopwatch sw = new Stopwatch();
            long totalElapsedMs = 0L;

            for (int i = 0; i < _loadTestCount; i++)
            {
                Guid deviceId = Guid.NewGuid();
                string wifiBssid = DataGenerator.GetRandomMacAddress();
                DeviceLicenseRequest deviceLicenseRequest = CreateDeviceLicenseRequest(deviceId, wifiBssid);

                using (var content = new ObjectContent<DeviceLicenseRequest>(deviceLicenseRequest, new JsonMediaTypeFormatter()))
                {
                    string requestUri = string.Format("{0}/{1}/{2}", ApiLoadEndpoint, ApiPut, deviceId);
                    var req = new HttpRequestMessage(HttpMethod.Put, requestUri)
                    {
                        Content = content
                    };
                    var client = new HttpClient();

                    sw.Start();

                    HttpResponseMessage response = client.SendAsync(req).Result;
                    bool ok = (HttpStatusCode.OK == response.StatusCode);

                    sw.Stop();

                    long elapsedMs = sw.ElapsedMilliseconds;
                    totalElapsedMs += elapsedMs;
                    Interlocked.Add(ref _totalElapsedMs, elapsedMs);
                    if (_verbose)
                    {
                        Debug.WriteLine("Thread {0} DevicesController Put load test #{1} returned {2} took {3}ms", threadNum, i, ok, elapsedMs);
                    }

                    sw.Reset();
                }
            }

            if (_verbose)
            {
                Debug.WriteLine("Thread {0} DevicesController Put {1} load tests took average {2}ms", threadNum, _loadTestCount, totalElapsedMs/_loadTestCount);
            }
        }

        private DeviceLicenseRequest CreateDeviceLicenseRequest(Guid deviceId, string wifiBssid)
        {
            return new DeviceLicenseRequest
            {
                DeviceId = deviceId.ToString(),
                WifiBSSID = wifiBssid,
                DownloadLicenseRequested = true,
                LearningContentQueued = 10,
                EnvironmentId = "ccsocdct",
                DeviceName = "iPad Simulator",
                UserId = "14a7b5f9-0cf4-4803-b62f-720eebd6986a",
                LocationId = "d622a7c1-d3b6-4236-9349-474ce92f2232",
                LocationName = "Alliance Collins Family College-Ready High School",
                Username = "John Weber",
                UserType = "teacher",
                ContentLastUpdatedAt = new DateTime(0),
                ConfiguredGrades = new List<int>(),
                ConfiguredUnitCount = 0,
                PSoCAppId = "com.pearson.commoncore.commoncore",
                DeviceOSVersion = "7.1",
                InstalledContentSize = 10,
                WifiSSID = "epic",
                PSoCAppVersion = "1.5.0",
                DeviceFreeSpace = 158157721600,
                CoursesInstalled = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "10"},
                            {"Percent", 0},
                            {"LearningResourceId", "0737c4b6-40f2-4c0e-b223-4977b03f9ae7"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "11"},
                            {"Percent", 0},
                            {"LearningResourceId", "aa26af81-40f2-43ad-9683-d0f13890f057"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "11"},
                            {"Percent", 0},
                            {"LearningResourceId", "00ada5dd-40f2-4713-9266-319591b76d6a"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "6"},
                            {"Percent", 0},
                            {"LearningResourceId", "0aac764c-40f2-4d28-a4f3-2b9953b760d2"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "6"},
                            {"Percent", 0},
                            {"LearningResourceId", "f84ecdfa-40f2-4854-bdb9-a0ee8bcda8cf"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "8"},
                            {"Percent", 0},
                            {"LearningResourceId", "f54fce7b-40f2-4bf1-9c04-858fbaac5ccf"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "8"},
                            {"Percent", 0},
                            {"LearningResourceId", "33236cc6-40f2-4157-9faf-4aa38d795130"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "9"},
                            {"Percent", 0},
                            {"LearningResourceId", "104ed877-40f2-41bb-8394-5364e849c6ec"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "9"},
                            {"Percent", 0},
                            {"LearningResourceId", "ff18e8cf-40f2-4450-a233-4cdb5b15d981"}
                        }
                    },
                DeviceType = "iPad Simulator"
            };
        }

/* TODO: Haven't gotten time to get this to work fully.  Could be removed if there are no future plans for running load test locally.
        [TestMethod]
        public async Task DevicesController_Put_InternalLoadTest()
        {
            Stopwatch sw = new Stopwatch();
            long totalElapsedMs = 0L;

            //IUnityContainer unityContainer = UnityConfig.GetConfiguredContainer();
            //UnityConfig.RegisterTypes(unityContainer);

            var container = new UnityContainer();
            container.RegisterType<IHttpClientFactory, Services.HttpClientFactory>();
            // Data
            container.RegisterType<IAccessPointRepository, AccessPointRepository>();
            container.RegisterType<IAdminRepository, AdminRepository>();
            container.RegisterType<IDeviceRepository, DeviceRepository>();
            container.RegisterType<IDeviceInstalledCourseRepository, DeviceInstalledCourseRepository>();
            container.RegisterType<IDistrictRepository, DistrictRepository>();
            container.RegisterType<ISchoolRepository, SchoolRepository>();
            container.RegisterType<ILicenseRepository, LicenseRepository>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            // Service
            container.RegisterType<IAccessPointService, AccessPointService>();
            container.RegisterType<IAdminService, AdminService>();
            container.RegisterType<IDeviceService, DeviceService>();
            container.RegisterType<IAdminAuthorizationService, AdminAuthorizationService>();
            container.RegisterType<IDistrictService, DistrictService>();
            container.RegisterType<ISchoolService, SchoolService>();
            container.RegisterType<ILicenseService, LicenseService>();
            container.RegisterType<ISchoolnetService, SchoolnetService>();
            container.RegisterType<IUserService, UserService>();

            var locator = new UnityServiceLocator(container);
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => locator);

            IDeviceService deviceService = container.Resolve<DeviceService>();
            ILicenseService licenseService = container.Resolve<LicenseService>();
            IUserService userService = container.Resolve<UserService>();
            DevicesController sut = new DevicesController(deviceService, licenseService, userService);
            
            for (int i = 0; i < LoadTestCount; i++)
            {
                Guid deviceId = Guid.NewGuid();
                var deviceLicenseRequest = new DeviceLicenseRequest
                {
                    DeviceId = deviceId.ToString(),
                    WifiBSSID = "2a:cf:e9:81:0b:00",
                    DownloadLicenseRequested = true,
                    LearningContentQueued = 10,
                    EnvironmentId = "ccsocdct",
                    DeviceName = "iPad Simulator",
                    UserId = "14a7b5f9-0cf4-4803-b62f-720eebd6986a",
                    LocationId = "d622a7c1-d3b6-4236-9349-474ce92f2232",
                    LocationName = "Alliance Collins Family College-Ready High School",
                    Username = "John Weber",
                    UserType = "teacher",
                    ContentLastUpdatedAt = new DateTime(0),
                    ConfiguredGrades = new List<int>(),
                    ConfiguredUnitCount = 0,
                    PSoCAppId = "com.pearson.commoncore.commoncore",
                    DeviceOSVersion = "7.1",
                    InstalledContentSize = 10,
                    WifiSSID = "epic",
                    PSoCAppVersion = "1.5.0",
                    DeviceFreeSpace = 158157721600,
                    CoursesInstalled = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "10"},
                            {"Percent", 0},
                            {"LearningResourceId", "0737c4b6-40f2-4c0e-b223-4977b03f9ae7"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "11"},
                            {"Percent", 0},
                            {"LearningResourceId", "aa26af81-40f2-43ad-9683-d0f13890f057"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "11"},
                            {"Percent", 0},
                            {"LearningResourceId", "00ada5dd-40f2-4713-9266-319591b76d6a"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "6"},
                            {"Percent", 0},
                            {"LearningResourceId", "0aac764c-40f2-4d28-a4f3-2b9953b760d2"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "6"},
                            {"Percent", 0},
                            {"LearningResourceId", "f84ecdfa-40f2-4854-bdb9-a0ee8bcda8cf"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "8"},
                            {"Percent", 0},
                            {"LearningResourceId", "f54fce7b-40f2-4bf1-9c04-858fbaac5ccf"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "8"},
                            {"Percent", 0},
                            {"LearningResourceId", "33236cc6-40f2-4157-9faf-4aa38d795130"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "ELA"},
                            {"Grade", "9"},
                            {"Percent", 0},
                            {"LearningResourceId", "104ed877-40f2-41bb-8394-5364e849c6ec"}
                        },
                        new Dictionary<string, object>
                        {
                            {"Subject", "Math"},
                            {"Grade", "9"},
                            {"Percent", 0},
                            {"LearningResourceId", "ff18e8cf-40f2-4450-a233-4cdb5b15d981"}
                        }
                    },
                    DeviceType = "iPad Simulator"
                };

                sw.Start();

                IHttpActionResult result = await sut.Put(deviceLicenseRequest.DeviceId, deviceLicenseRequest).ConfigureAwait(false);
                Debug.WriteLine("DevicesController Put load test #{0} result is {1}", i, result);

                sw.Stop();

                long elapsedMs = sw.ElapsedMilliseconds;
                totalElapsedMs += elapsedMs;
                Debug.WriteLine("DevicesController Put load test #{0} took {1}ms", i, elapsedMs);

                sw.Reset();
            }

            Debug.WriteLine("DevicesController Put {0} load tests took average {1}ms", LoadTestCount, totalElapsedMs / LoadTestCount);
        }
 */
    }
}
