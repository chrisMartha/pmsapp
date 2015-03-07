using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.UnitTest
{
    [TestClass]
    public class AdminAuthorizationServiceTest
    {
        private AdminAuthorizationService _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new AdminAuthorizationService();
        }

        #region IsAuthorized (AdminType) tests
        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_NullToGlobal_Failure()
        {
            // Arrange

            // Act
            bool result = _sut.IsAuthorized(null, AdminType.GlobalAdmin);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_GlobalToGlobal_Success()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, AdminType.GlobalAdmin);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_GlobalToDistrict_Success()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, AdminType.DistrictAdmin);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_GlobalToSchool_Success()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, AdminType.SchoolAdmin);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_DistrictToGlobal_Success()
        {
            // Arrange
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto()
            };

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, AdminType.GlobalAdmin);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_DistrictToDistrict_Success()
        {
            // Arrange
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto()
            };

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, AdminType.DistrictAdmin);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_DistrictToSchool_Success()
        {
            // Arrange
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto()
            };

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, AdminType.SchoolAdmin);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_SchoolToGlobal_Success()
        {
            // Arrange
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto()
            };

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, AdminType.GlobalAdmin);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_SchoolToDistrict_Success()
        {
            // Arrange
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto()
            };

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, AdminType.DistrictAdmin);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_AdminType_SchoolToSchool_Success()
        {
            // Arrange
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto()
            };

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, AdminType.SchoolAdmin);

            // Assert
            Assert.IsTrue(result);
        }
        #endregion

        #region IsAuthorized (District) tests
        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_NullAdmin_Failure()
        {
            // Arrange
            Guid districtId = Guid.NewGuid();

            // Act
            bool result = _sut.IsAuthorized(null, districtId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_GlobalAdmin_Success()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();
            Guid districtId = Guid.NewGuid();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, districtId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_ThisDistrictAdmin_Success()
        {
            // Arrange
            Guid districtId = Guid.NewGuid();
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto
                {
                    DistrictId = districtId
                }
            };

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, districtId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_OtherDistrictAdmin_Failure()
        {
            // Arrange
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto
                {
                    DistrictId = Guid.NewGuid()
                }
            };
            Guid districtId = Guid.NewGuid();

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, districtId);

            // Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_SchoolAdmin_Failure()
        {
            // Arrange
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto()
            };
            Guid districtId = Guid.NewGuid();

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, districtId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_District_DefaultDistrict_Failure()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();
            Guid districtId = default(Guid);

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, districtId);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region IsAuthorized (School) tests
        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_NullAdmin_Failure()
        {
            // Arrange
            School school = new School();

            // Act
            bool result = _sut.IsAuthorized(null, school);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_GlobalAdmin_Success()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();
            School school = new School();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, school);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_ThisDistrictAdmin_Success()
        {
            // Arrange
            Guid districtId = Guid.NewGuid();
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto
                {
                    DistrictId = districtId
                }
            };
            School school = new School
            {
                District = new District
                {
                    DistrictId = districtId
                }
            };

            // Act
            bool result = _sut.IsAuthorized(districtAdmin, school);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_OtherDistrictAdmin_Failure()
        {
            // Arrange
            AdminDto districtAdmin = new AdminDto
            {
                District = new DistrictDto
                {
                    DistrictId = Guid.NewGuid()
                }
            };
            School school = new School
            {
                District = new District
                {
                    DistrictId = Guid.NewGuid()
                }
            };


            // Act
            bool result = _sut.IsAuthorized(districtAdmin, school);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_ThisSchoolAdmin_Success()
        {
            // Arrange
            Guid schoolId = Guid.NewGuid();
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto
                {
                    SchoolID = schoolId
                }
            };
            School school = new School
            {
                SchoolId = schoolId
            };

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, school);

            // Assert
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_OtherSchoolAdmin_Success()
        {
            // Arrange
            AdminDto schoolAdmin = new AdminDto
            {
                School = new SchoolDto
                {
                    SchoolID = Guid.NewGuid()
                }
            };
            School school = new School
            {
                SchoolId = Guid.NewGuid()
            };

            // Act
            bool result = _sut.IsAuthorized(schoolAdmin, school);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AdminAuthorizationService_IsAuthorized_School_NullSchool_Failure()
        {
            // Arrange
            AdminDto globalAdmin = new AdminDto();

            // Act
            bool result = _sut.IsAuthorized(globalAdmin, null);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion
    }
}
