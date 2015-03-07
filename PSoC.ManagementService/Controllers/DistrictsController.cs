using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Models;
using PSoC.ManagementService.Services.Models.Schoolnet;

namespace PSoC.ManagementService.Controllers
{
    /// <summary>
    /// Web API 2.0 controller with POST actions to import district and schools data
    /// </summary>
    public class DistrictsController : ControllerBase
    {
        private readonly IDistrictService _districtService;
        private readonly ISchoolService _schoolService;

        /// <summary>
        /// Using DI create and initialize Web API 2.0 districts controller
        /// </summary>
        /// <param name="districtService">Business layer district service</param>
        /// <param name="schoolService">Business layer school service</param>
        public DistrictsController(IDistrictService districtService, ISchoolService schoolService)
        {
            _districtService = districtService;
            _schoolService = schoolService;
        }

        /// <summary>
        /// Endpoint to import district data
        /// </summary>
        /// <param name="response">District data in Schoolnet response format</param>
        /// <param name="oAuthApplicationId">Districts' OAuth application id</param>
        /// <param name="oAuthClientId">Districts' OAuth client id</param>
        /// <param name="oAuthUrl">Districts' OAuth URL</param>
        /// <returns>200 if successful, 400 if errors validating data or inserting it into the database</returns>
        [HttpPost]
        [Route("api/v1/districts")]
        public async Task<IHttpActionResult> Post([FromBody]DistrictResponse response, 
            string oAuthApplicationId = "c23cd56c-0fd0-4de6-8b8f-97e4332d9eea",
            string oAuthClientId = "42b1233a-2020-4bb2-8f5f-78daff0cb84d",
            string oAuthUrl = "https://schoolnet-dct.ccsocdev.net/")
        {
            List<string> validationErrors = response.Validate();
            if (validationErrors != null && validationErrors.Count > 0)
            {
                string message = string.Empty;
                message = validationErrors.Aggregate(message, (current, validationError) => current + (validationError + Environment.NewLine));
                return BadRequest(message);
            }

            Guid validOAuthApplicationId;
            if (!Guid.TryParse(oAuthApplicationId, out validOAuthApplicationId))
            {
                string message = string.Format("OAuthApplicationId {0} is not a valid GUID", oAuthApplicationId);
                return BadRequest(message);
            }

            Guid validOAuthClientId;
            if (!Guid.TryParse(oAuthClientId, out validOAuthClientId))
            {
                string message = string.Format("OAuthClientId {0} is not a valid GUID", oAuthClientId);
                return BadRequest(message);
            }

            Uri validOAuthUrl;
            if (!Uri.TryCreate(oAuthUrl, UriKind.Absolute, out validOAuthUrl))
            {
                string message = string.Format("OAuthUrl {0} is not a well-formed URI", oAuthUrl);
                return BadRequest(message);
            }

            const string securePrefix = "https://";
            if (oAuthUrl.Substring(0, securePrefix.Length) != securePrefix)
            {
                string message = string.Format("OAuth {0} is not secure", oAuthUrl);
                return BadRequest(message);
            }

            List<string> operationErrors = new List<string>();
            foreach (Institution institution in response.Institutions)
            {
                District district = await _districtService.GetByIdAsync(institution.InstitutionId).ConfigureAwait(false);
                if (district == null)
                {
                    if (string.IsNullOrWhiteSpace(institution.InstitutionName))
                    {
                        operationErrors.Add(string.Format("Institution name for district id {0} is required.", institution.InstitutionId));
                        break;
                    }

                    district = new District
                    {
                        DistrictId = institution.InstitutionId,
                        DistrictName = institution.InstitutionName,
                        CreatedBy = "DistrictsController",
                        DistrictAnnotation = string.Format("externalId = {0}", institution.ExternalId),
                        OAuthApplicationId = oAuthApplicationId,
                        OAuthClientId = oAuthClientId,
                        OAuthUrl = oAuthUrl
                    };
                    try
                    {
                        District newDistrict = await _districtService.CreateAsync(district).ConfigureAwait(false);
                        if (newDistrict == null)
                        {
                            operationErrors.Add(string.Format("Failed to add district id {0}.", institution.InstitutionId));
                        }
                    }
                    catch (Exception e)
                    {
                        operationErrors.Add(string.Format("Failed to add district id {0}. Exception: {1}", institution.InstitutionId, e));
                    }
                }
                else
                {
                    operationErrors.Add(string.Format("District with id {0} already exists.", institution.InstitutionId));
                }
            }

            if (operationErrors.Count > 0)
            {
                string message = string.Empty;
                message = operationErrors.Aggregate(message, (current, operationError) => current + (operationError + Environment.NewLine));
                return BadRequest(message);
            }

            return Ok();
        }

        /// <summary>
        /// Endpoint to import schools data
        /// </summary>
        /// <param name="districtId">District id as Schoolnet institution id</param>
        /// <param name="response">District data in Schoolnet response format</param>
        /// <returns>200 if successful, 400 if errors validating data or inserting it into the database, 404 if district is not found</returns>
        [HttpPost]
        [Route("api/v1/districts/{districtId}/schools")]
        public async Task<IHttpActionResult> Post(Guid districtId, [FromBody]SchoolsResponse response)
        {
            List<string> validationErrors = response.Validate();
            if (validationErrors != null && validationErrors.Count > 0)
            {
                string message = string.Empty;
                message = validationErrors.Aggregate(message, (current, validationError) => current + (validationError + Environment.NewLine));
                return BadRequest(message);
            }

            District district;
            try
            {
                district = await _districtService.GetByIdAsync(districtId).ConfigureAwait(false);
                if (district == null)
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }

            List<string> operationErrors = new List<string>();
            int successCount = 0;
            foreach (Institution institution in response.Institutions)
            {
                School school = await _schoolService.GetByIdAsync(institution.InstitutionId).ConfigureAwait(false);
                if (school == null)
                {
                    school = new School
                    {
                        SchoolId = institution.InstitutionId,
                        District = district,
                        SchoolName = institution.InstitutionName,
                        SchoolAnnotation = string.Format("externalId = {0}", institution.ExternalId),
                    };
                    try
                    {
                        School newSchool = await _schoolService.CreateAsync(school).ConfigureAwait(false);
                        if (newSchool == null)
                        {
                            operationErrors.Add(string.Format("Failed to add school id {0}.", institution.InstitutionId));
                        }

                        successCount++;
                    }
                    catch (Exception e)
                    {
                        operationErrors.Add(string.Format("Failed to add school id {0}. Exception: {1}", institution.InstitutionId, e));
                    }
                }
                else
                {
                    operationErrors.Add(string.Format("School with id {0} already exists.", institution.InstitutionId));
                }
            }

            if (operationErrors.Count > 0)
            {
                string message = string.Empty;
                
                // If at least some schools were imported, note that in the message
                if (successCount > 0)
                {
                    message = string.Format("Successfully imported {0} schools.{1}", successCount, Environment.NewLine);
                }

                message += operationErrors.Aggregate(message, (current, operationError) => current + (operationError + Environment.NewLine));

                return BadRequest(message);
            }

            return Ok();
        }

    }
}
