using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSoC.ManagementService.Data.Models;
using PSoC.ManagementService.Data.Repositories;

namespace PSoC.ManagementService.Data.IntegrationTests.Repositories
{
    [TestClass]
    public class ClassroomRepositoryTest
    {
        private ClassroomRepository _sut;

        [TestMethod]
        public async Task ClassroomRepository_DeleteAsync_ThrowsNotImplementedException()
        {
            Exception thrownException = null;
            try
            {
                bool result =  await _sut.DeleteAsync(Guid.NewGuid()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            // Assert
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(NotImplementedException));
        }

        [TestMethod]
        public async Task ClassroomRepository_DeleteManyAsync_ThrowsNotImplementedException()
        {
            Exception thrownException = null;
            try
            {
                await _sut.DeleteAsync(new [] { Guid.NewGuid() }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            // Assert
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(NotImplementedException));
        }

        [TestMethod]
        public async Task ClassroomRepository_GetAsync()
        {

            var result = await _sut.GetAsync().ConfigureAwait(false);

            Assert.IsNotNull(result);

        }

        [TestMethod]
        public async Task ClassroomRepository_GetByIdAsync_ReturnsNull()
        {
            var result = await _sut.GetByIdAsync(Guid.NewGuid()).ConfigureAwait(false);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ClassroomRepository_InsertAsync_ThrowsSqlException()
        {
            Exception thrownException = null;
            try
            {
                await _sut.InsertAsync(new ClassroomDto()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            // Assert
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SqlException));
        }

        [TestMethod]
        public async Task ClassroomRepository_UpdateAsync_ThrowsNotImplementedException()
        {
            Exception thrownException = null;
            try
            {
                await _sut.UpdateAsync(new ClassroomDto()).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                thrownException = e;
            }

            // Assert
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(NotImplementedException));
        }

        [TestInitialize]
        public void Initialize()
        {
            _sut = new ClassroomRepository();
        }
    }
}