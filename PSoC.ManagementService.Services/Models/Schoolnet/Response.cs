using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace PSoC.ManagementService.Services.Models.Schoolnet
{
    /// <summary>
    /// Represents response sent by SchoolNet API and containing informationg about district, e.g. https://schoolnet.ccsocdev.net/api/v1/districts
    /// </summary>
    public class DistrictResponse : Response
    {
        public DistrictResponse() : base(District)
        {
        }
    }

    /// <summary>
    /// Represents response sent by SchoolNet API and containing information about schools, e.g. https://schoolnet.ccsocdev.net/api/v1/districts/{districtId}/schools
    /// </summary>
    public class SchoolsResponse : Response
    {
        public SchoolsResponse() : base(School)
        {
        }
    }

    /// <summary>
    /// Represents a generic response sent by SchoolNet API to certain endpoints, e.g. https://schoolnet.ccsocdev.net/api/v1/districts
    /// </summary>
    public abstract class Response
    {
        public const string Success = "Success";
        public const string District = "District";
        public const string School = "School";

        private readonly int[] _validCodes = { (int) HttpStatusCode.OK, (int) HttpStatusCode.PartialContent };
        private readonly string _validInstitutionType;

        [JsonProperty("code")]
        public int Code { private get; set; }

        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("status")]
        public string Status { private get; set; }

        [JsonProperty("data")]
        public Institution[] Institutions { get; set; }

        protected Response(string validInstitutionType)
        {
            _validInstitutionType = validInstitutionType;
        }

        public List<string> Validate()
        {
            List<string> validationErrors = new List<string>();

            bool isValidCode = _validCodes.Any(validCode => Code == validCode);
            if (!isValidCode)
            {
                string validCodesString = string.Empty;
                validCodesString = _validCodes.Aggregate(validCodesString, (current, validCode) => current + (validCode + ", "));
                char[] endChars = {',', ' '};
                validCodesString = validCodesString.TrimEnd(endChars);
                validationErrors.Add(string.Format("Code value {0} is not valid. Allowed values: {1}.", Code, validCodesString));
            }

            if (Status != Success)
            {
                validationErrors.Add(string.Format("Status value {0} is not valid. Allowed value: {1}.", Status, Success));
            }

            validationErrors.AddRange(from institution in Institutions
                                      where institution.InstitutionType != _validInstitutionType
                                      select string.Format("Institution id {0} type value {1} is not valid. Allowed value: {2}.", institution.InstitutionId, institution.InstitutionType, _validInstitutionType));

            return validationErrors;
        }
    }
}
