using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Repositories;

namespace PSoC.ManagementService.Data.IntegrationTests.Repositories
{
    [TestClass]
    public class ConfigCodeRepositoryTest
    {
        private ConfigCodeRepository _sut;
        private ConfigCodeDto _configCodeToDelete;
        private ConfigCodeDto _configCodeToDeleteMany1;
        private ConfigCodeDto _configCodeToDeleteMany2;
        private ConfigCodeDto _configCodeToUpdate;
        private Guid _testUser = new Guid("c89ccc4e-edb6-43c9-9dbe-e46cdb845fae");

        [TestMethod]
        public async Task ConfigCodeRepository_Delete_Success()
        {
            // Act
            var result = await _sut.DeleteAsync(_configCodeToDelete.ConfigCodeID).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);

            var actual = await _sut.GetByIdAsync(_configCodeToDelete.ConfigCodeID).ConfigureAwait(false);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task ConfigCodeRepository_DeleteAsync_InvalidId()
        {
            // Act
            Boolean result = await _sut.DeleteAsync(Guid.NewGuid()).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ConfigCodeRepository_DeleteMany_Success()
        {
            var ids = new Guid[] { _configCodeToDeleteMany1.ConfigCodeID, _configCodeToDeleteMany2.ConfigCodeID };
            // Act
            var result = await _sut.DeleteAsync(ids).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);

            var actual1 = await _sut.GetByIdAsync(_configCodeToDeleteMany1.ConfigCodeID).ConfigureAwait(false);
            Assert.IsNull(actual1);

            var actual2 = await _sut.GetByIdAsync(_configCodeToDeleteMany2.ConfigCodeID).ConfigureAwait(false);
            Assert.IsNull(actual2);
        }

        [TestMethod]
        public async Task ConfigCodeRepository_Update_Success()
        {
            var dto = new ConfigCodeDto
            {
                Active = true,
                ConfigCodeAnnotation = "Updated 1",
                ConfigCodeID = _configCodeToUpdate.ConfigCodeID,
                ConfigCodeName = "Updated",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };

            var result = await _sut.UpdateAsync(dto).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dto.ConfigCodeID, result.ConfigCodeID);

            Assert.AreNotEqual(_configCodeToUpdate.ConfigCodeAnnotation, result.ConfigCodeAnnotation);
            Assert.AreEqual("Updated 1", result.ConfigCodeAnnotation);

            Assert.AreNotEqual(_configCodeToUpdate.ConfigCodeName, result.ConfigCodeName);
            Assert.AreEqual("Updated", result.ConfigCodeName);

        }

        [TestMethod]
        public async Task ConfigCodeRepository_Insert_Success()
        {
            var dto = new ConfigCodeDto
            {
                Active = false,
                ConfigCodeAnnotation = "New config",
                ConfigCodeID = Guid.NewGuid(),
                ConfigCodeName = "New",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };


            InitIntegrationTest.TestConfigCodeIds.Add(dto.ConfigCodeID);
            var result = await _sut.InsertAsync(dto).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(dto.ConfigCodeID, result.ConfigCodeID);
            Assert.AreEqual(dto.ConfigCodeAnnotation, result.ConfigCodeAnnotation);
            Assert.AreEqual(dto.ConfigCodeName, result.ConfigCodeName);
            Assert.AreEqual(dto.District.DistrictId, result.District.DistrictId);
            Assert.AreEqual(dto.CreatedBy.UserID, result.CreatedBy.UserID);
        }
        
        [TestMethod]
        public async Task ConfigCodeRepository_GetAsync_Success()
        {
            var result = await _sut.GetAsync().ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 2);
        }

        [TestMethod]
        public async Task ConfigCodeRepository_GetByIdAsync_Success()
        {
            var id = new Guid("00000000-0000-4F8C-8653-000000000001");
            var result = await _sut.GetByIdAsync(id).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.ConfigCodeID);
        }

        [TestMethod]
        public async Task ConfigCodeRepository_GetByDistrictIdAsync_Success()
        {
            var id = InitIntegrationTest.TestDistrictId;
            var result = await _sut.GetByDistrictIdAsync(id).ConfigureAwait(false);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 2);
        }

        [TestInitialize]
        public void Initialize()
        {
            // Arrange
            _sut = new ConfigCodeRepository();

            _configCodeToDelete = new ConfigCodeDto
            {
                Active = false,
                ConfigCodeAnnotation = "Delete me",
                ConfigCodeID = Guid.NewGuid(),
                ConfigCodeName = "DeleteMe",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };

            _configCodeToDeleteMany1 = new ConfigCodeDto
            {
                Active = false,
                ConfigCodeAnnotation = "Delete me1",
                ConfigCodeID = Guid.NewGuid(),
                ConfigCodeName = "DeleteMe1",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };

            _configCodeToDeleteMany2 = new ConfigCodeDto
            {
                Active = false,
                ConfigCodeAnnotation = "Delete me2",
                ConfigCodeID = Guid.NewGuid(),
                ConfigCodeName = "DeleteMe2",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };

            _configCodeToUpdate = new ConfigCodeDto
            {
                Active = false,
                ConfigCodeAnnotation = "Update me",
                ConfigCodeID = Guid.NewGuid(),
                ConfigCodeName = "UpdateMe",
                CreatedBy = new UserDto { UserID = _testUser },
                District = new DistrictDto { DistrictId = InitIntegrationTest.TestDistrictId }
            };


            InitIntegrationTest.TestConfigCodeIds.Add(_configCodeToDelete.ConfigCodeID);
            _sut.InsertAsync(_configCodeToDelete).Wait();

            InitIntegrationTest.TestConfigCodeIds.Add(_configCodeToDeleteMany1.ConfigCodeID);
            _sut.InsertAsync(_configCodeToDeleteMany1).Wait();

            InitIntegrationTest.TestConfigCodeIds.Add(_configCodeToDeleteMany2.ConfigCodeID);
            _sut.InsertAsync(_configCodeToDeleteMany2).Wait();

            InitIntegrationTest.TestConfigCodeIds.Add(_configCodeToUpdate.ConfigCodeID);
            _sut.InsertAsync(_configCodeToUpdate).Wait();
        }
    }
}
