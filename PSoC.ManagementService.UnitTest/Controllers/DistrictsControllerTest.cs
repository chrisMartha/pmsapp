using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PSoC.ManagementService.Controllers;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Models;
using PSoC.ManagementService.Services.Models.Schoolnet;

namespace PSoC.ManagementService.UnitTest.Controllers
{
    [TestClass]
    public class DistrictsControllerTest
    {
        private const string DistrictId = "8141c935-3213-4c38-8342-65348e1cd97b";
        private const string DistrictName = "National School District";
        private const string SchoolId = "008392ec-417c-4bae-8740-ff4891237cac";

        private Mock<IDistrictService> _districtServiceMock;
        private Mock<ISchoolService> _schoolServiceMock;
        private DistrictsController _sut;

        [TestInitialize]
        public void Initialize()
        {
            _districtServiceMock = new Mock<IDistrictService>();
            _schoolServiceMock = new Mock<ISchoolService>();
            _sut = new DistrictsController(_districtServiceMock.Object, _schoolServiceMock.Object);
        }

        #region api/v1/districts tests
        [TestMethod]
        public async Task DistrictsController_Post_ValidDistrictsResponse_OkResult()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            District district = new District();
            _districtServiceMock.Setup(x => x.CreateAsync(It.IsAny<District>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task DistrictsController_Post_InvalidOAuthApplicationId_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            const string oAuthApplicationId = "Invalid OAuth Application Id";
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            District district = new District();
            _districtServiceMock.Setup(x => x.CreateAsync(It.IsAny<District>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse, oAuthApplicationId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("OAuthApplicationId {0} is not a valid GUID", oAuthApplicationId));
        }

        [TestMethod]
        public async Task DistrictsController_Post_InvalidOAuthClientId_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            const string oAuthClientId = "Invalid OAuth Client Id";
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            District district = new District();
            _districtServiceMock.Setup(x => x.CreateAsync(It.IsAny<District>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse, oAuthClientId: oAuthClientId).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("OAuthClientId {0} is not a valid GUID", oAuthClientId));
        }

        [TestMethod]
        public async Task DistrictsController_Post_InvalidOAuthUrl_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            const string oAuthUrl = "Invalid OAuth URL";
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            District district = new District();
            _districtServiceMock.Setup(x => x.CreateAsync(It.IsAny<District>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse, oAuthUrl: oAuthUrl).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("OAuthUrl {0} is not a well-formed URI", oAuthUrl));
        }

        [TestMethod]
        public async Task DistrictsController_Post_UnsecureOAuthUrl_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            const string oAuthUrl = "http://schoolnet-dct.ccsocdev.net/";
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            District district = new District();
            _districtServiceMock.Setup(x => x.CreateAsync(It.IsAny<District>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse, oAuthUrl: oAuthUrl).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("OAuth {0} is not secure", oAuthUrl));
        }

        [TestMethod]
        public async Task DistrictsController_Post_ExistingDistrict_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, DistrictName, Response.District);
            District district = new District();
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(district);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("District with id {0} already exists.{1}", DistrictId, Environment.NewLine));
        }

        [TestMethod]
        public async Task DistrictsController_Post_NullInstitutionName_BadRequest()
        {
            // Arrange
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, null, Response.District);
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("Institution name for district id {0} is required.{1}", DistrictId, Environment.NewLine));
        }

        [TestMethod]
        public async Task DistrictsController_Post_EmptyInstitutionName_BadRequest()
        {
            // Arrange
            string districtName = string.Empty;
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, districtName, Response.District);
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("Institution name for district id {0} is required.{1}", DistrictId, Environment.NewLine));
        }

        [TestMethod]
        public async Task DistrictsController_Post_BlankInstitutionName_BadRequest()
        {
            // Arrange
            const string districtName = "     ";
            DistrictResponse districtResponse = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, DistrictId, districtName, Response.District);
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);

            // Act
            IHttpActionResult result = await _sut.Post(districtResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("Institution name for district id {0} is required.{1}", DistrictId, Environment.NewLine));
        }

        private DistrictResponse CreateDistrictResponse(HttpStatusCode httpStatusCode, string status, string institutionId, string institutionName, string institutionType)
        {
            var response = new DistrictResponse
            {
                Code = (int) httpStatusCode,
                RequestId = new Guid("9bbd92a3-9818-44ee-8bac-3c779d4eafef"),
                Status = status,
                Institutions = new[]
                {
                    new Institution
                    {
                        InstitutionId = new Guid(institutionId),
                        InstitutionName = institutionName,
                        ExternalId = "sw_300",
                        InstitutionType = institutionType,
                        Links = new []
                        {
                            new Link
                            {
                                Relationship = "GetSchools",
                                Url = string.Format("https://schoolnet.ccsocdev.net/api/v1/districts/{0}/schools", institutionId)
                            }
                        }
                    }
                }
            };

            return response;
        }
        #endregion

        #region api/v1/districts/{districtId}/schools tests
        [TestMethod]
        public async Task DistrictsController_Post_ValidSchoolResponse_OkResult()
        {
            // Arrange
            Guid districtId = new Guid(DistrictId);
            SchoolsResponse schoolsResponse = CreateSchoolsResponse(HttpStatusCode.PartialContent, Response.Success, SchoolId, Response.School);
            District district = new District();
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(district);
            _schoolServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);
            School school = new School();
            _schoolServiceMock.Setup(x => x.CreateAsync(It.IsAny<School>())).ReturnsAsync(school);

            // Act
            IHttpActionResult result = await _sut.Post(districtId, schoolsResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task DistrictsController_Post_UnknownDistrict_NotFound()
        {
            // Arrange
            Guid districtId = Guid.NewGuid();
            SchoolsResponse schoolsResponse = CreateSchoolsResponse(HttpStatusCode.PartialContent, Response.Success, SchoolId, Response.School);
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null);

            // Act
            IHttpActionResult result = await _sut.Post(districtId, schoolsResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task DistrictsController_Post_ExistingSchool_BadRequest()
        {
            // Arrange
            Guid districtId = new Guid(DistrictId);
            SchoolsResponse schoolsResponse = CreateSchoolsResponse(HttpStatusCode.PartialContent, Response.Success, SchoolId, Response.School);
            District district = new District();
            _districtServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(district);
            School school = new School();
            _schoolServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(school);

            // Act
            IHttpActionResult result = await _sut.Post(districtId, schoolsResponse).ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            BadRequestErrorMessageResult badRequest = result as BadRequestErrorMessageResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(badRequest.Message, string.Format("School with id {0} already exists.{1}", SchoolId, Environment.NewLine));
        }

        private SchoolsResponse CreateSchoolsResponse(HttpStatusCode httpStatusCode, string status, string institutionId, string institutionType)
        {
            var response = new SchoolsResponse
            {
                Code = (int) httpStatusCode,
                RequestId = new Guid("e8eb0920-3776-42c5-bf16-fbcb7cffa34a"),
                Status = status,
                Institutions = new[]
                {
                    new Institution
                    {
                        InstitutionId = new Guid(institutionId),
                        InstitutionName = "W. C. BRUNK (VAN)",
                        ExternalId = "965",
                        InstitutionType = institutionType,
                        Links = new []
                        {
                            new Link
                            {
                                Relationship = "GetSections",
                                Url = string.Format("https://schoolnet.ccsocdev.net/api/v1/schools/{0}/sections", institutionId)
                            }
                        }
                    }
                }
            };

            return response;
        }
        #endregion
    }
}
