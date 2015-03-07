using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSoC.ManagementService.Services.Models.Schoolnet;

namespace PSoC.ManagementService.Services.UnitTest.Models.Schoolnet
{
    [TestClass]
    public class ResponseTest
    {
        private const string InstitutionId = "8141c935-3213-4c38-8342-65348e1cd97b";

        [TestMethod]
        public void Response_Validate_OkResponse_Success()
        {
            // Arrange
            DistrictResponse response = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, InstitutionId, Response.District);

            // Act
            List<string> validationErrors = response.Validate();

            // Assert
            Assert.IsNotNull(validationErrors);
            Assert.AreEqual(validationErrors.Count, 0);
        }

        [TestMethod]
        public void Response_Validate_PartialContentResponse_Success()
        {
            // Arrange
            DistrictResponse response = CreateDistrictResponse(HttpStatusCode.PartialContent, Response.Success, InstitutionId, Response.District);

            // Act
            List<string> validationErrors = response.Validate();

            // Assert
            Assert.IsNotNull(validationErrors);
            Assert.AreEqual(validationErrors.Count, 0);
        }

        [TestMethod]
        public void Response_Validate_InvalidCodeResponse_Failure()
        {
            // Arrange
            const HttpStatusCode invalidCode = HttpStatusCode.Ambiguous;
            DistrictResponse response = CreateDistrictResponse(invalidCode, Response.Success, InstitutionId, Response.District);

            // Act
            List<string> validationErrors = response.Validate();

            // Assert
            Assert.IsNotNull(validationErrors);
            Assert.AreEqual(validationErrors.Count, 1);
            Assert.AreEqual(validationErrors[0], string.Format("Code value {0} is not valid. Allowed values: {1}, {2}.", (int) invalidCode, (int) HttpStatusCode.OK, (int) HttpStatusCode.PartialContent));
        }

        [TestMethod]
        public void Response_Validate_InvalidStatusResponse_Failure()
        {
            // Arrange
            const string invalidStatus = "Failure";
            DistrictResponse response = CreateDistrictResponse(HttpStatusCode.OK, invalidStatus, InstitutionId, Response.District);

            // Act
            List<string> validationErrors = response.Validate();

            // Assert
            Assert.IsNotNull(validationErrors);
            Assert.AreEqual(validationErrors.Count, 1);
            Assert.AreEqual(validationErrors[0], string.Format("Status value {0} is not valid. Allowed value: {1}.", invalidStatus, Response.Success));
        }

        [TestMethod]
        public void Response_Validate_InvalidInstitutionTypeResponse_Failure()
        {
            // Arrange
            const string invalidInstitutionType = Response.School;
            DistrictResponse response = CreateDistrictResponse(HttpStatusCode.OK, Response.Success, InstitutionId, invalidInstitutionType);

            // Act
            List<string> validationErrors = response.Validate();

            // Assert
            Assert.IsNotNull(validationErrors);
            Assert.AreEqual(validationErrors.Count, 1);
            Assert.AreEqual(validationErrors[0], string.Format("Institution id {0} type value {1} is not valid. Allowed value: {2}.", InstitutionId, Response.School, Response.District));
        }

        private DistrictResponse CreateDistrictResponse(HttpStatusCode httpStatusCode, string status, string institutionId, string institutionType)
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
                        InstitutionName = "National School District",
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
    }
}
