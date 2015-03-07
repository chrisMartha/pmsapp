using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.SearchFilter;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.UnitTest
{
    [TestClass]
    public class DeviceServiceTest
    {
        private Mock<IDeviceInstalledCourseRepository> _deviceInstalledCourseRepositoryMock;
        private Mock<IAccessPointDeviceStatusRepository> _accessPointDeviceStatusRepositoryMock;
        private DeviceService _sut;

        [TestInitialize]
        public void TestInit()
        {
            _deviceInstalledCourseRepositoryMock = new Mock<IDeviceInstalledCourseRepository>();
            _accessPointDeviceStatusRepositoryMock = new Mock<IAccessPointDeviceStatusRepository>();
            _deviceInstalledCourseRepositoryMock
                .Setup(x => x.ImportDataAsync(It.IsAny<Guid>(), It.IsAny<List<DeviceInstalledCourseDto>>()))
                .ReturnsAsync(true);
            _accessPointDeviceStatusRepositoryMock
                .Setup(x => x.GetByAdminTypeAsync(It.IsAny<AdminType>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<IReadOnlyDictionary<FilterType, SearchFilter>>()))
                .ReturnsAsync(new Tuple<List<DeviceDto>, int>(new List<DeviceDto>(), 0));
            _sut = new DeviceService(_deviceInstalledCourseRepositoryMock.Object,
                _accessPointDeviceStatusRepositoryMock.Object);
        }

        [TestMethod]
        public async Task TestSaveDownloadStatusAsync_valid()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var courseId1 = Guid.NewGuid();
            var courseId2 = Guid.NewGuid();
            var courses = new List<Course>
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
            // Act
            await _sut.SaveDownloadStatusAsync(deviceId, courses).ConfigureAwait(false);
            // Assert
            _deviceInstalledCourseRepositoryMock.Verify(
                x => x.ImportDataAsync(It.IsAny<Guid>(), It.IsAny<List<DeviceInstalledCourseDto>>()), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAccessPointDeviceStatusAsync_GlobalAdmin()
        {
            // Arrange
            const AdminType globalAdminType = AdminType.GlobalAdmin;

            // Act
            var list = await _sut.GetAccessPointDeviceStatusAsync(globalAdminType, null, 10, 0).ConfigureAwait(false);
            // Assert 
            _accessPointDeviceStatusRepositoryMock.Verify(
                x =>
                    x.GetByAdminTypeAsync(globalAdminType, null, 10, 0,
                        It.IsAny<IReadOnlyDictionary<FilterType, SearchFilter>>()), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAccessPointDeviceStatusAsync_DistrictAdmin()
        {
            // Arrange
            const AdminType districtAdminType = AdminType.DistrictAdmin;

            // Act
            var list =
                await
                    _sut.GetAccessPointDeviceStatusAsync(districtAdminType, Guid.NewGuid(), 10, 0).ConfigureAwait(false);

            // Assert 
            _accessPointDeviceStatusRepositoryMock.Verify(
                x =>
                    x.GetByAdminTypeAsync(districtAdminType, It.IsAny<Guid>(), 10, 0,
                        It.IsAny<IReadOnlyDictionary<FilterType, SearchFilter>>()), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAccessPointDeviceStatusAsync_SchoolAdmin()
        {
            // Arrange
            const AdminType schoolAdminType = AdminType.SchoolAdmin;

            // Act
            var list =
                await _sut.GetAccessPointDeviceStatusAsync(schoolAdminType, Guid.NewGuid(), 10, 0).ConfigureAwait(false);

            // Assert 
            _accessPointDeviceStatusRepositoryMock.Verify(
                x =>
                    x.GetByAdminTypeAsync(schoolAdminType, It.IsAny<Guid>(), 10, 0,
                        It.IsAny<IReadOnlyDictionary<FilterType, SearchFilter>>()), Times.Once);
        }

        [TestMethod]
        public async Task TestGetAccessPointDeviceStatusAsync_GlobalAdmin_MultiFiltersShouldNotThrowException()
        {
            // Arrange
            const AdminType globalAdminType = AdminType.GlobalAdmin;
            Exception thrownException = null;
            IList<Guid> districtIds = new List<Guid>() { new Guid() };
            IList<Guid> schoolIds = new List<Guid>() { new Guid() };

            IReadOnlyDictionary<FilterType, SearchFilter> searchFilters =
                new Dictionary<FilterType, SearchFilter>()
                {
                    {FilterType.SchoolId, new SchoolFilter(schoolIds)},
                    {FilterType.DistrictId, new DistrictFilter(districtIds)}
                };

            _accessPointDeviceStatusRepositoryMock.Setup(x => x.GetByAdminTypeAsync(globalAdminType, null, 10, 0, searchFilters))
                .ReturnsAsync(new Tuple<List<DeviceDto>, int>(new List<DeviceDto>(), 0));

            try
            {
                // Act
                var list =
                    await _sut.GetAccessPointDeviceStatusAsync(globalAdminType, null, 10, 0).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            // Assert 
            Assert.IsNull(thrownException);
        }
    }
}
