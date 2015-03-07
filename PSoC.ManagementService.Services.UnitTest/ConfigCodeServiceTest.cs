using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Services.UnitTest
{
    [TestClass]
    public class ConfigCodeServiceTest
    {
        private Mock<IAdminRepository> _adminRepositoryMock;
        private Mock<IConfigCodeRepository> _configCodeRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IAdminAuthorizationService> _adminAuthorizationServiceMock;
        private ConfigCodeService _sut;
        private const string testUsername = "Test User";
        private readonly Guid testUserId = Guid.Parse("aaaaaaaa-C439-0000-ABCC-000000000001");
        private readonly Guid testDistrictId = Guid.Parse("dddddddd-C439-0000-ABCC-000000000001");
        private readonly Guid testDistrictId2 = Guid.Parse("dddddddd-C439-0000-ABCC-000000000002");
        private readonly Guid testId = Guid.Parse("00000000-C439-0000-ABCC-000000000001");
        private readonly Guid testId2 = Guid.Parse("00000000-C439-0000-ABCC-000000000002");

        [TestInitialize]
        public void Initialize()
        {
            _adminRepositoryMock = new Mock<IAdminRepository>();
            _configCodeRepositoryMock = new Mock<IConfigCodeRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _adminAuthorizationServiceMock = new Mock<IAdminAuthorizationService>();

            // Pre-arrange
            _unitOfWorkMock.Setup(x => x.GetDataRepository<AdminDto, Guid>()).Returns(_adminRepositoryMock.Object);
            _unitOfWorkMock.Setup(x => x.GetDataRepository<ConfigCodeDto, Guid>()).Returns(_configCodeRepositoryMock.Object);
            _unitOfWorkMock.SetupGet(x => x.AdminRepository).Returns(_adminRepositoryMock.Object);

            // Subject under test
            _sut = new ConfigCodeService(_unitOfWorkMock.Object, _configCodeRepositoryMock.Object, _adminAuthorizationServiceMock.Object);
        }

        [TestMethod]
        public void ConfigCodeService_CastConfigCodeDtoToConfigCode()
        {
            // Arrange
            var configCodeDto = GenerateTestConfigCodeDto();

            // Act
            var configCode = (ConfigCode)configCodeDto;

            // Assert
            Assert.AreEqual(configCodeDto.ConfigCodeID, configCode.ConfigCodeID);
            Assert.AreEqual(configCodeDto.Active, configCode.Active);
            Assert.AreEqual(configCodeDto.ConfigCodeAnnotation, configCode.ConfigCodeAnnotation);
            Assert.AreEqual(configCodeDto.ConfigCodeName, configCode.ConfigCodeName);
            Assert.AreEqual(configCodeDto.CreatedBy.UserID, configCode.CreatedByUserId);
            Assert.AreEqual(configCodeDto.CreatedBy.Username, configCode.CreatedByUserName);
            Assert.AreEqual(configCodeDto.District.DistrictId, configCode.DistrictId);
            Assert.AreEqual(configCodeDto.District.DistrictName, configCode.DistrictName);
        }

        [TestMethod]
        public void ConfigCodeService_CastConfigCodeToConfigCodeDto()
        {
            // Arrange
            var configCode = new ConfigCode
            {
                ConfigCodeID = testId,
                Active = true,
                ConfigCodeAnnotation = "Sample",
                ConfigCodeName = "SampleCode",
                CreatedByUserId = testUserId,
                DistrictId = testDistrictId 
            };

            // Act
            var ConfigCodeDto = (ConfigCodeDto)configCode;

            // Assert
            Assert.AreEqual(ConfigCodeDto.ConfigCodeID, configCode.ConfigCodeID);
            Assert.AreEqual(ConfigCodeDto.Active, configCode.Active);
            Assert.AreEqual(ConfigCodeDto.ConfigCodeAnnotation, configCode.ConfigCodeAnnotation);
            Assert.AreEqual(ConfigCodeDto.ConfigCodeName, configCode.ConfigCodeName);
            Assert.AreEqual(ConfigCodeDto.CreatedBy.UserID, configCode.CreatedByUserId);
            Assert.AreEqual(ConfigCodeDto.District.DistrictId, configCode.DistrictId);
            Assert.AreEqual(ConfigCodeDto.District.DistrictName, configCode.DistrictName);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_ReturnsNone()
        {
            // Arrange
            var ConfigCodes = new List<ConfigCodeDto>();
            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes);

            // Act
            var result = await _sut.GetAsync().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_ReturnsMany()
        {
            // Arrange
            var ConfigCode1 = GenerateTestConfigCodeDto(1);
            var ConfigCode2 = GenerateTestConfigCodeDto(2);  
            
            var ConfigCodes = new List<ConfigCodeDto>
            {
                ConfigCode1,
                ConfigCode2
            };

            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes);

            // Act
            var result = await _sut.GetAsync().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(r => r.ConfigCodeID == ConfigCode1.ConfigCodeID));
            Assert.IsTrue(result.Any(r => r.ConfigCodeID == ConfigCode2.ConfigCodeID));
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_GlobalAdmin_ReturnsMany()
        {
            // Arrange
            var globalAdmin = new AdminDto();
            var ConfigCode1 = GenerateTestConfigCodeDto(1);
            var ConfigCode2 = GenerateTestConfigCodeDto(2); 
            
            var ConfigCodes = new List<ConfigCodeDto>
            {
                ConfigCode1,
                ConfigCode2
            };

            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(globalAdmin);
            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(ConfigCode1);

            // Act
            var result = await _sut.GetAsync().ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(r => r.ConfigCodeID == ConfigCode1.ConfigCodeID));
            Assert.IsTrue(result.Any(r => r.ConfigCodeID == ConfigCode2.ConfigCodeID));
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_DistrictAdmin_ReturnsOne()
        {
            // Arrange
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId }, User = new UserDto { UserID = testUserId } };
            var ConfigCode1 = GenerateTestConfigCodeDto(1);
            var ConfigCode2 = GenerateTestConfigCodeDto(2);

            var ConfigCodes1 = new List<ConfigCodeDto>
            {
                ConfigCode1
            };

            var ConfigCodes2 = new List<ConfigCodeDto>
            {
                ConfigCode1,
                ConfigCode2
            };

            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, AdminType.DistrictAdmin)).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes2);
            _configCodeRepositoryMock.Setup(x => x.GetByDistrictIdAsync(testDistrictId)).ReturnsAsync(ConfigCodes1);

            // Act
            var result = await _sut.GetAsync(testUsername).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(r => r.ConfigCodeID == ConfigCode1.ConfigCodeID));
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_SchoolAdmin_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var schoolAdmin = new AdminDto { School = new SchoolDto() };
            var ConfigCode1 = GenerateTestConfigCodeDto(1);
            var ConfigCode2 = GenerateTestConfigCodeDto(2); 
            
            var ConfigCodes = new List<ConfigCodeDto>
            {
                ConfigCode1,
                ConfigCode2
            };
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(schoolAdmin);
            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(ConfigCode1);

            // Act
            try
            {
                await _sut.GetAsync(testUsername).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetAsync_NonAdmin_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var ConfigCode1 = GenerateTestConfigCodeDto(1);
            var ConfigCode2 = GenerateTestConfigCodeDto(2); 
            
            var ConfigCodes = new List<ConfigCodeDto>
            {
                ConfigCode1,
                ConfigCode2
            };
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(null);
            _configCodeRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(ConfigCodes);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(ConfigCode1);

            // Act
            try
            {
                await _sut.GetAsync(testUsername).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_NonExistentId_Failure()
        {
            // Arrange
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(null);

            // Act
            var result = await _sut.GetByIdAsync(testId).ConfigureAwait(false);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_ExistingId_Success()
        {
            // Arrange
            var configCode = GenerateTestConfigCodeDto(); 

            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            var result = await _sut.GetByIdAsync(testId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_GlobalAdmin_ExistingId_Success()
        {
            // Arrange
            var globalAdmin = new AdminDto();
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(globalAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(globalAdmin, It.IsAny<Guid>())).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            var result = await _sut.GetByIdAsync(testUsername, testId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_DistrictAdmin_ExistingId_Success()
        {
            // Arrange
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId }, User = new UserDto { UserID = testUserId } };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, It.IsAny<Guid>())).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            var result = await _sut.GetByIdAsync(testUsername, testId).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_InvalidDistrictAdmin_ExistingId_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId2 } };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.GetByIdAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_SchoolAdmin_ExistingId_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var schoolAdmin = new AdminDto { School = new SchoolDto() };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(schoolAdmin);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.GetByIdAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_GetByIdAsync_NonAdmin_ExistingId_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(null);
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(testId)).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.GetByIdAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_NullEntity_Failure()
        {
            // Arrange
            ConfigCodeDto ConfigCode = null;
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(ConfigCode)).ReturnsAsync(null);

            Exception expectedException = null;
            // Act
            try
            {
                await _sut.CreateAsync(null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_ValidEntity_Success()
        {
            // Arrange
            var configCode = GenerateTestConfigCodeDto();
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.CreateAsync((ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_GlobalAdmin_ValidEntity_Success()
        {
            // Arrange
            var globalAdmin = new AdminDto();
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(globalAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(globalAdmin, It.IsAny<Guid>())).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.CreateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_DistrictAdmin_ValidEntity_Success()
        {
            // Arrange
            const bool success = true;
            var districtAdmin = new AdminDto();
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, It.IsAny<Guid>())).Returns(success);
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.CreateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_InvalidDistrictAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId2 } };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.CreateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_SchoolAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var schoolAdmin = new AdminDto { School = new SchoolDto() };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(schoolAdmin);
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.CreateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_CreateAsync_NonAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(null);
            _configCodeRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.CreateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_NullEntity_Failure()
        {
            // Arrange
            ConfigCodeDto ConfigCode = null;
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(ConfigCode)).ReturnsAsync(null);

            Exception expectedException = null;
            // Act
            try
            {
                await _sut.UpdateAsync(null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
           
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_ValidEntity_Success()
        {
            // Arrange
            var configCode = GenerateTestConfigCodeDto();
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.UpdateAsync((ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_GlobalAdmin_ValidEntity_Success()
        {
            // Arrange
            var globalAdmin = new AdminDto();
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(globalAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(globalAdmin, It.IsAny<Guid>())).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.UpdateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_DistrictAdmin_ValidEntity_Success()
        {
            // Arrange
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId }, User = new UserDto { UserID = testUserId } };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, It.IsAny<Guid>())).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            var result = await _sut.UpdateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(configCode.ConfigCodeID, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_InvalidDistrictAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId2 } };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.UpdateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_SchoolAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var schoolAdmin = new AdminDto { School = new SchoolDto() };
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(schoolAdmin);
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.UpdateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_UpdateAsync_NonAdmin_ValidEntity_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            var configCode = GenerateTestConfigCodeDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(null);
            _configCodeRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ConfigCodeDto>())).ReturnsAsync(configCode);

            // Act
            try
            {
                await _sut.UpdateAsync(testUsername, (ConfigCode)configCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_NonExistingKey_Failure()
        {
            // Arrange
            const bool success = false;
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            var result = await _sut.DeleteAsync(testId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(success, result);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_ExistingKey_Success()
        {
            // Arrange
            const bool success = true;
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            var result = await _sut.DeleteAsync(testId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(success, result);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_GlobalAdmin_ExistingKey_Success()
        {
            // Arrange
            const bool success = true;
            var globalAdmin = new AdminDto();
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(globalAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(globalAdmin, testDistrictId)).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            var result = await _sut.DeleteAsync(testUsername, testId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(success, result);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_DistrictAdmin_ExistingKey_Success()
        {
            // Arrange
            const bool success = true;
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId }, User = new UserDto { UserID = testUserId } };
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, testDistrictId)).Returns(true);
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            var result = await _sut.DeleteAsync(testUsername, testId).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(success, result);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_InvalidDistrictAdmin_ExistingKey_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            const bool success = true;
            var districtAdmin = new AdminDto { District = new DistrictDto { DistrictId = testDistrictId2 } };
            var configCode = GenerateTestConfigCodeDto();
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(configCode);
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(districtAdmin);
            _adminAuthorizationServiceMock.Setup(x => x.IsAuthorized(districtAdmin, testDistrictId)).Returns(false);
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            try
            {
                await _sut.DeleteAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_SchoolAdmin_ExistingKey_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            const bool success = true;
            var schoolAdmin = new AdminDto { School = new SchoolDto() };
            var configCode = GenerateTestConfigCodeDto();
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(configCode);
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(schoolAdmin);
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            try
            {
                await _sut.DeleteAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public async Task ConfigCodeService_DeleteAsync_NonAdmin_ExistingKey_ThrowException()
        {
            // Arrange
            Exception expectedException = null;
            const bool success = true;
            var configCode = GenerateTestConfigCodeDto();
            _configCodeRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(configCode);
            _adminRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<String>())).ReturnsAsync(null);
            _configCodeRepositoryMock.Setup(x => x.DeleteAsync(testId)).ReturnsAsync(success);

            // Act
            try
            {
                await _sut.DeleteAsync(testUsername, testId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            Assert.IsNotNull(expectedException);
        }

        private ConfigCodeDto GenerateTestConfigCodeDto(int index = 0)
        {
            ConfigCodeDto ConfigCodeDto;
            switch (index)
            {
                case 1:
                    ConfigCodeDto = new ConfigCodeDto
                    {
                        ConfigCodeID = testId,
                        Active = true,
                        ConfigCodeAnnotation = "Sample1",
                        ConfigCodeName = "SampleCode1",
                        CreatedBy = new UserDto { UserID = testUserId, Username = testUsername },
                        District = new DistrictDto { DistrictId = testDistrictId, DistrictName = "Test District" }
                    };
                    break;
                case 2:
                    ConfigCodeDto = new ConfigCodeDto
                    {
                        ConfigCodeID = testId2,
                        Active = false,
                        ConfigCodeAnnotation = "Sample2",
                        ConfigCodeName = "SampleCode2",
                        CreatedBy = new UserDto { UserID = testUserId, Username = testUsername },
                        District = new DistrictDto { DistrictId = testDistrictId, DistrictName = "Test District" }
                    };
                    break;
                default:
                    ConfigCodeDto = new ConfigCodeDto
                    {
                        ConfigCodeID = testId,
                        Active = true,
                        ConfigCodeAnnotation = "Sample",
                        ConfigCodeName = "SampleCode",
                        CreatedBy = new UserDto { UserID = testUserId, Username = testUsername },
                        District = new DistrictDto { DistrictId = testDistrictId, DistrictName = "Test District" }
                    };
                    break;
            }

            return ConfigCodeDto;
            
        }
    }
}
