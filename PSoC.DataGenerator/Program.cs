using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Repositories;

using CommandLine;
using CommandLine.Text;

namespace PSoC.DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (options.NumberOfDistrictsToAdd > 0 && !string.IsNullOrWhiteSpace(options.DistrictsToAdd))
                {
                    Console.WriteLine("Can not use both arguments -d and -c");
                    Console.WriteLine(options.GetUsage());
                    return;
                }
                else
                {
                    LoadData(options).Wait();
                    Console.WriteLine("Done");
                }
            }
        }

        static async Task LoadData(Options options)
        {
            string sqlCommand = "";

            sqlCommand = string.Format(@"SELECT
                             [PSocSchoolId]
                            ,[PSocDistrictId]
                            ,[NCES_School_ID]
                            ,[State_School_ID]
                            ,[NCES_District_ID]
                            ,[State_District_ID]
                            ,[Low_Grade]
                            ,[High_Grade]
                            ,[Grades]
                            ,[School_Name]
                            ,[District]
                            ,[County_Name]
                            ,[Street_Address]
                            ,[City]
                            ,[State]
                            ,[ZIP]
                            ,[ZIP_4digit]
                            ,[Phone]
                            ,[Locale_Code]
                            ,[Locale]
                            ,[Charter]
                            ,[Magnet]
                            ,[Title_I_School]
                            ,[Title_1_School_Wide]
                            ,[Students]
                            ,[Teachers]
                            ,[Student_Teacher_Ratio]
                            ,[Free_Lunch]
                            ,[Reduced_Lunch] FROM [dbo].[USSchoolData]
                            WHERE [PSocDistrictId] in (SELECT Distinct TOP {0} [PSocDistrictId] FROM [dbo].[USSchoolData])
                            ", options.NumberOfDistrictsToAdd);

            ICollection<USSchoolData> schoolData = await GetSchoolData(sqlCommand).ConfigureAwait(false);

            if (options.LicenseRequestToAdd > 0)
            {
                await AddLicenseRequestAsync(options, schoolData).ConfigureAwait(false);
            }
            else if (options.NumberOfDistrictsToAdd > 0)
            {

                await AddDistrictsAsync(schoolData).ConfigureAwait(false);
                await AddSchoolsAsync(schoolData).ConfigureAwait(false);
                await AddClassroomAsync(schoolData).ConfigureAwait(false);
                await AddUsersAsync(schoolData).ConfigureAwait(false);
                await AddDevicesAsync(schoolData).ConfigureAwait(false);
                
            }


        }

        private static async Task AddLicenseRequestAsync(Options options, ICollection<USSchoolData> schoolData)
        {
            var deviceRepository = new DeviceRepository();
            var licenseRepository = new LicenseRepository();
            var accessPointRepository = new AccessPointRepository();
            var userRepository = new UserRepository();

            Console.WriteLine("Loading Devices");
            var devices = await deviceRepository.GetIdsAsync().ConfigureAwait(false);

            Console.WriteLine("Loading AccessPoints");
            var accessPoints = await accessPointRepository.GetAsync().ConfigureAwait(false);

            Console.WriteLine("Loading Users");
            var users = await userRepository.GetIdsAsync().ConfigureAwait(false);


            int devicesCount = devices.Count;
            int consoleTop = 4;

            int index = 0;
            AccessPointDto[] filteredAccessPoints = null;
            Guid prevSchoolId = new Guid();
            int accesspointIndex = 0;
            int userIndex = 0;
            DateTime requestDateTime = DateTime.UtcNow;

            if (options.LicenseRequestToAdd > devicesCount)
            {               

                for (int x = 0; x < options.LicenseRequestToAdd; x++)
                {
                    if (index >= devicesCount) index = 0;

                    var l = await licenseRepository.GetLicenseForDeviceAsync(devices[index].DeviceID).ConfigureAwait(false);
                    LicenseRequestDto lr = null;

                    if (l == null)
                    {
                        if (devices[index].School != null)
                        {
                            devices[index].LastUsedConfigCode = "psocdev";

                            if (filteredAccessPoints == null || (devices[index].School.SchoolID != prevSchoolId))
                            {
                                filteredAccessPoints = accessPoints.Where(a => a.School != null && a.School.SchoolID == devices[index].School.SchoolID).ToArray();
                                if (!filteredAccessPoints.Any())
                                    continue;

                                prevSchoolId = devices[index].School.SchoolID;
                                accesspointIndex = 0;
                            }

                            if (accesspointIndex >= filteredAccessPoints.Length)
                            {
                                accesspointIndex = 0;
                                requestDateTime = requestDateTime.AddMinutes(60);
                            }

                            if (userIndex >= users.Count) userIndex = 0;

                            lr = new LicenseRequestDto
                            {
                                Device = devices[index],
                                School = devices[index].School,
                                AccessPoint = filteredAccessPoints[accesspointIndex],
                                LicenseRequestID = Guid.NewGuid(),
                                ConfigCode = "psocdev",
                                User = users[userIndex],
                                RequestDateTime = requestDateTime
                            };

                            accesspointIndex++;
                            userIndex++;
                        }
                    }
                    else
                    {
                        lr = l.LicenseRequest;
                        lr.License = l;
                        lr.ConfigCode = "psocdev";
                        lr.Device.LastUsedConfigCode = "psocdev";

                        if (l.AccessPoint == null)
                        {
                            if (accesspointIndex >= filteredAccessPoints.Length)
                            {
                                accesspointIndex = 0;
                                requestDateTime = requestDateTime.AddMinutes(60);
                            }

                            lr.AccessPoint = filteredAccessPoints[accesspointIndex];
                        }
                        else
                        {
                            lr.AccessPoint = l.AccessPoint;
                        }

                        lr.RequestDateTime = requestDateTime;

                    }

                    var result = await licenseRepository.GrantLicenseForDeviceAsync(lr).ConfigureAwait(false);


                    index++;
                    Console.CursorTop = consoleTop;
                    Console.WriteLine("LicenseRequest Added: {0}", x);

                }
            }
            else
            {
                

                for (int x = 0; x < options.LicenseRequestToAdd; x++)
                {
                    var l = await licenseRepository.GetLicenseForDeviceAsync(devices[x].DeviceID).ConfigureAwait(false);
                    LicenseRequestDto lr = null;

                    if (l == null)
                    {
                        if (devices[x].School != null)
                        {

                            devices[x].LastUsedConfigCode = "psocdev";

                            if (filteredAccessPoints == null || (devices[x].School.SchoolID != prevSchoolId))
                            {
                                filteredAccessPoints = accessPoints.Where(a => a.School != null && a.School.SchoolID == devices[x].School.SchoolID).ToArray();
                                if (filteredAccessPoints.Count() <= 0)
                                    continue;
                                prevSchoolId = devices[index].School.SchoolID;
                                accesspointIndex = 0;
                            }

                            if (accesspointIndex >= filteredAccessPoints.Length)
                            {
                                accesspointIndex = 0;
                                requestDateTime = requestDateTime.AddMinutes(60);
                            }

                            if (userIndex >= users.Count) userIndex = 0;

                            lr = new LicenseRequestDto
                            {
                                Device = devices[x],
                                School = devices[x].School,
                                AccessPoint = filteredAccessPoints[accesspointIndex],
                                LicenseRequestID = Guid.NewGuid(),
                                ConfigCode = "psocdev",
                                User = users[userIndex],
                                RequestDateTime = requestDateTime
                            };

                            accesspointIndex++;
                            userIndex++;
                        }
                    }
                    else
                    {
                        lr = l.LicenseRequest;
                        lr.License = l;
                        lr.ConfigCode = "psocdev";
                        lr.Device.LastUsedConfigCode = "psocdev";

                        if (l.AccessPoint == null)
                        {
                            if (accesspointIndex >= filteredAccessPoints.Length)
                            {
                                accesspointIndex = 0;
                                requestDateTime = requestDateTime.AddMinutes(60);
                            }

                            lr.AccessPoint = filteredAccessPoints[accesspointIndex];
                        }
                        else
                        {
                            lr.AccessPoint = l.AccessPoint;
                        }

                        lr.RequestDateTime = requestDateTime;
                    }

                    var result = await licenseRepository.GrantLicenseForDeviceAsync(lr).ConfigureAwait(false);

                    Console.CursorTop = consoleTop;
                    Console.WriteLine("LicenseRequest Added: {0}", x);
                }
            }
        }

        private static async Task AddClassroomAsync(ICollection<USSchoolData> schoolData)
        {
            var classRepository = new ClassroomRepository();
            var wifiRepository = new AccessPointRepository();

            List<string> usedWifies = new List<string>();


            int consoleTop = 6;

            int count = 0;
            foreach (var s in schoolData)
            {
                double teachers = s.Teachers.HasValue ? Convert.ToDouble(s.Teachers.Value) : s.Students.HasValue ? Convert.ToDouble(s.Students.Value / 20) : 0;

                if (teachers > 0)
                {
                    int classrooms = Convert.ToInt32(teachers *= 1.1);
                    int maxStudentsPerClassroom = 36;

                    for (int x = 1; x <= classrooms; x++)
                    {
                        var classroom = new ClassroomDto
                        {
                            BuildingName = s.School_Name,
                            ClassroomAnnotation = "Sample Data",
                            ClassroomID = Guid.NewGuid(),
                            ClassroomName = (x + 100).ToString("000"),
                            School = new SchoolDto { SchoolID = s.PSocSchoolId, District = new DistrictDto { DistrictId = s.PSocDistrictId } }
                        };

                        await classRepository.InsertAsync(classroom).ConfigureAwait(false);

                        count++;
                        if (count % 100 == 0)
                        {
                            Console.CursorTop = consoleTop;
                            Console.WriteLine("Classrooms Added: {0}", count);
                        }

                        var mac1 = GetRandomMacAddress();
                        var mac2 = GetRandomMacAddress();

                        while (usedWifies.Contains(mac1))
                        {
                            mac1 = GetRandomMacAddress();
                        }

                        usedWifies.Add(mac1);

                        while (usedWifies.Contains(mac2))
                        {
                            mac2 = GetRandomMacAddress();
                        }

                        usedWifies.Add(mac2);

                        var wifi1 = new AccessPointDto
                        {
                            AccessPointAnnotation = "Sample Data",
                            AccessPointExpiryTimeSeconds = 1200,
                            AccessPointMaxDownloadLicenses = maxStudentsPerClassroom,
                            Classroom = new ClassroomDto { ClassroomID = classroom.ClassroomID },
                            District = new DistrictDto { DistrictId = s.PSocDistrictId },
                            School = new SchoolDto { SchoolID = s.PSocSchoolId },
                            WifiSSID = Faker.Lorem.GetFirstWord(),
                            WifiBSSID = mac1
                        };

                        await wifiRepository.InsertAsync(wifi1).ConfigureAwait(false);

                        var wifi2 = new AccessPointDto
                        {
                            AccessPointAnnotation = "Sample Data",
                            AccessPointExpiryTimeSeconds = 1200,
                            AccessPointMaxDownloadLicenses = maxStudentsPerClassroom,
                            Classroom = new ClassroomDto { ClassroomID = classroom.ClassroomID },
                            District = new DistrictDto { DistrictId = s.PSocDistrictId },
                            School = new SchoolDto { SchoolID = s.PSocSchoolId },
                            WifiSSID = Faker.Lorem.GetFirstWord(),
                            WifiBSSID = mac2
                        };

                        await wifiRepository.InsertAsync(wifi2).ConfigureAwait(false);
                    }
                }
            }

            Console.CursorTop = consoleTop;
            Console.WriteLine("Classrooms Added: {0}", count);
        }

        private static async Task AddUsersAsync(ICollection<USSchoolData> schoolData)
        {
            System.Threading.Thread.Sleep(2000);

            var repository = new UserRepository();
            int consoleTop = 1;

            int totalUsers = schoolData.Sum(s => (s.Teachers.HasValue ? Convert.ToInt32(s.Teachers.Value) : 0) + (s.Students.HasValue ? Convert.ToInt32(s.Students.Value) : 0));

            int count = 0;

            foreach (var s in schoolData)
            {
                int students = s.Students.HasValue ? Convert.ToInt32(s.Students) : 0;
                int teachers = s.Teachers.HasValue ? Convert.ToInt32(s.Teachers) : 0;

                for (int x = 1; x <= students; x++)
                {
                    var user = new UserDto
                    {
                        UserID = Guid.NewGuid(),
                        Username = Faker.Name.FullName(Faker.NameFormats.Standard).Replace(" ", "") + x.ToString(),
                        UserType = "Student"
                    };

                    Task t = repository.InsertAsync(user);
                    count++;
                    if (count % 10 == 0)
                    {
                        Console.CursorTop = consoleTop;
                        Console.WriteLine("Users Added: {0}", count);
                    }

                    if (count % 10000 == 0 || x >= students)
                    {
                        Task.WaitAll(t);
                        System.Threading.Thread.Sleep(200);
                        System.Threading.Thread.Sleep(0);
                        System.Threading.Thread.SpinWait(200);

                    }
                }

                for (int x = 1; x <= teachers; x++)
                {
                    var user = new UserDto
                    {
                        UserID = Guid.NewGuid(),
                        Username = Faker.Name.FullName(Faker.NameFormats.Standard).Replace(" ", "") + x.ToString(),
                        UserType = "Teacher"
                    };

                    repository.InsertAsync(user);

                    count++;
                    if (count % 10 == 0)
                    {
                        Console.CursorTop = consoleTop;
                        Console.WriteLine("Users Added: {0}", count);
                    }
                }
            }

            Console.CursorTop = consoleTop;
            Console.WriteLine("Users Added: {0}", count);
        }

        private static async Task AddDevicesAsync(ICollection<USSchoolData> schoolData)
        {
            System.Threading.Thread.Sleep(1000);

            var repository = new DeviceRepository();

            int consoleTop = 2;

            int count = 0;

            foreach (var s in schoolData)
            {
                int totalUsers = (s.Teachers.HasValue ? Convert.ToInt32(s.Teachers.Value) : 0) + (s.Students.HasValue ? Convert.ToInt32(s.Students.Value) : 0);

                for (int x = 1; x <= totalUsers; x++)
                {
                    var device = new DeviceDto
                    {
                        DeviceID = Guid.NewGuid(),
                        DeviceName = string.Format("{0}-{1}", count, Faker.Name.First()),
                        School = new SchoolDto { SchoolID = s.PSocSchoolId, District = new DistrictDto { DistrictId = s.PSocDistrictId } }

                    };

                    Task t = repository.InsertAsync(device);

                    count++;
                    if (count % 10 == 0)
                    {
                        Console.CursorTop = consoleTop;
                        Console.WriteLine("Devices Added: {0}", count);
                    }

                    if (count % 10000 == 0 || x >= totalUsers)
                    {
                        Task.WaitAll(t);
                        System.Threading.Thread.Sleep(1000);
                        System.Threading.Thread.Sleep(0);
                        System.Threading.Thread.SpinWait(1000);

                    }
                }

            }

            Console.CursorTop = consoleTop;
            Console.WriteLine("Devices Added: {0}", count);
        }

        private static async Task AddSchoolsAsync(ICollection<USSchoolData> schoolData)
        {
            var repository = new SchoolRepository();

            int consoleTop = 8;

            int count = 0;

            foreach (var s in schoolData)
            {
                int maxDownloadLicenses = (s.Teachers.HasValue ? Convert.ToInt32(s.Teachers.Value) : 0) + (s.Students.HasValue ? Convert.ToInt32(s.Students.Value) : 0) + 5;
                var school = new SchoolDto
                {
                    SchoolID = s.PSocSchoolId,
                    District = new DistrictDto { DistrictId = s.PSocDistrictId },
                    SchoolAddress1 = s.Street_Address,
                    SchoolGrades = s.Grades,
                    SchoolName = s.School_Name,
                    SchoolState = s.State,
                    SchoolTown = s.City,
                    SchoolZipCode = s.ZIP + "-" + s.ZIP_4digit,
                    SchoolAnnotation = "Sample Data",
                    SchoolLicenseExpirySeconds = 2400,
                    SchoolMaxDownloadLicenses = maxDownloadLicenses
                };

                try
                {
                    await repository.InsertAsync(school).ConfigureAwait(false);
                    count++;
                    if (count % 2 == 0)
                    {
                        Console.CursorTop = consoleTop;
                        Console.WriteLine("Schools Added: {0}", count);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.CursorTop = consoleTop;
                Console.WriteLine("Schools Added: {0}", count);
            }
        }

        private static async Task AddDistrictsAsync(ICollection<USSchoolData> schoolData)
        {

            var repository = new DistrictRepository();

            var districts = schoolData.GroupBy(s => new { DistrictId = s.PSocDistrictId, DistrictName = s.District }).Select(x => x);

            int consoleTop = 7;

            int count = 0;
            foreach (var d in districts)
            {
                int maxDownloadLicenses = 10;
                var schools = schoolData.Where(s => s.PSocDistrictId == d.Key.DistrictId);

                if (schools != null)
                {
                    int buffer = schools.Count() * 5;
                    maxDownloadLicenses = buffer + schools.Sum(s => Convert.ToInt32((s.Teachers.HasValue ? s.Teachers.Value : 0) + (s.Students.HasValue ? s.Students.Value : 0)));
                }

                var district = new DistrictDto
                {
                    DistrictId = d.Key.DistrictId,
                    DistrictName = d.Key.DistrictName,
                    DistrictMaxDownloadLicenses = maxDownloadLicenses,
                    DistrictLicenseExpirySeconds = 3600,
                    DistrictAnnotation = "Sample Data",
                    OAuthApplicationId = "c23cd56c-0fd0-4de6-8b8f-97e4332d9eea",
                    OAuthClientId = "42b1233a-2020-4bb2-8f5f-78daff0cb84d",
                    OAuthURL = "https://schoolnet-dct.ccsocdev.net/"
                };

                try
                {
                    await repository.InsertAsync(district).ConfigureAwait(false);
                    count++;
                    Console.CursorTop = consoleTop;
                    Console.WriteLine("Districts Added: {0}", count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        private static async Task<ICollection<USSchoolData>> GetSchoolData(string query)
        {
            List<USSchoolData> data = new List<USSchoolData>();
            using (SqlDataReader dr = await DataAccessHelper.GetDataReaderAsync(query).ConfigureAwait(false))
            {
                if (dr.HasRows)
                {
                    while (await dr.ReadAsync().ConfigureAwait(false))
                    {
                        var s = new USSchoolData
                        {
                            PSocSchoolId = dr.GetGuid(0),
                            PSocDistrictId = dr.GetGuid(1),
                            NCES_School_ID = dr.IsDBNull(2) ? null : dr.GetString(2),
                            State_School_ID = dr.IsDBNull(3) ? null : dr.GetString(3),
                            NCES_District_ID = dr.IsDBNull(4) ? null : dr.GetString(4),
                            State_District_ID = dr.IsDBNull(5) ? null : dr.GetString(5),
                            Low_Grade = dr.IsDBNull(6) ? null : dr.GetString(6),
                            High_Grade = dr.IsDBNull(7) ? null : dr.GetString(7),
                            Grades = dr.IsDBNull(8) ? null : dr.GetString(8),
                            School_Name = dr.IsDBNull(9) ? null : dr.GetString(9),
                            District = dr.IsDBNull(10) ? null : dr.GetString(10),
                            County_Name = dr.IsDBNull(11) ? null : dr.GetString(11),
                            Street_Address = dr.IsDBNull(12) ? null : dr.GetString(12),
                            City = dr.IsDBNull(13) ? null : dr.GetString(13),
                            State = dr.IsDBNull(14) ? null : dr.GetString(14),
                            ZIP = dr.IsDBNull(15) ? null : dr.GetString(15),
                            ZIP_4digit = dr.IsDBNull(16) ? null : dr.GetString(16),
                            Phone = dr.IsDBNull(17) ? null : dr.GetString(17),
                            Locale_Code = dr.IsDBNull(18) ? null : dr.GetString(18),
                            Locale = dr.IsDBNull(19) ? null : dr.GetString(19),
                            Charter = dr.IsDBNull(20) ? null : dr.GetString(20),
                            Magnet = dr.IsDBNull(21) ? null : dr.GetString(21),
                            Title_I_School = dr.IsDBNull(22) ? null : dr.GetString(22),
                            Title_1_School_Wide = dr.IsDBNull(23) ? null : dr.GetString(23),
                            Students = dr.IsDBNull(24) ? default(decimal?) : dr.GetDecimal(24),
                            Teachers = dr.IsDBNull(25) ? default(decimal?) : dr.GetDecimal(25),
                            Student_Teacher_Ratio = dr.IsDBNull(26) ? default(decimal?) : dr.GetDecimal(26),
                            Free_Lunch = dr.IsDBNull(27) ? default(int?) : dr.GetInt32(27),
                            Reduced_Lunch = dr.IsDBNull(28) ? default(int?) : dr.GetInt32(28),
                        };

                        data.Add(s);
                    }
                }
            }

            return data;
        }

        internal static string GetRandomMacAddress()
        {
            var g = Guid.NewGuid().ToByteArray();
            var buffer = new byte[6];

            buffer[0] = g[1];
            buffer[1] = g[15];
            buffer[2] = g[2];
            buffer[3] = g[8];
            buffer[4] = g[14];
            buffer[5] = g[0];

            var result = String.Join(":", buffer.Select(x => string.Format("{0}", x.ToString("X2"))).ToArray());
            return result;
        }
    }
}
