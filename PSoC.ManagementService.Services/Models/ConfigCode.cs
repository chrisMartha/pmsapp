using System;
using System.ComponentModel.DataAnnotations;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.Extensions;
using PSoC.ManagementService.Data.Models;


namespace PSoC.ManagementService.Services.Models
{
    public class ConfigCode
    {
        [Required]
        [Display(Name = "ID")]
        public Guid ConfigCodeID { get; set; }

        //TODO: Make sure the model is validated at the controller.
        [Required]
        [StringLength(10)]
        [RegularExpression("[a-zA-Z0-9]*")]
        [Display(Name = "Name")]
        public string ConfigCodeName { get; set; }

        public bool Active { get; set; }

        public DateTime Created { get; set; }

        [StringLength(80)]
        [Display(Name = "Annotation")]
        public string ConfigCodeAnnotation { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Display(Name = "Created By")]
        public string CreatedByUserName { get; set; }

        [Required]
        [Display(Name = "District ID")]
        public Guid DistrictId { get; set; }

        [Display(Name = "District")]
        public string DistrictName { get; set; }

        public static explicit operator ConfigCode(ConfigCodeDto configCode)
        {
            if (configCode == null) return null;

            return new ConfigCode()
            {
                Active = configCode.Active,
                ConfigCodeAnnotation = configCode.ConfigCodeAnnotation,
                ConfigCodeID = configCode.ConfigCodeID,
                ConfigCodeName = configCode.ConfigCodeName,
                Created = configCode.Created,
                CreatedByUserId = configCode.CreatedBy.UserID,
                CreatedByUserName = string.IsNullOrWhiteSpace(configCode.CreatedBy.Username) ? null : configCode.CreatedBy.Username,
                DistrictId = configCode.District.DistrictId,
                DistrictName = string.IsNullOrWhiteSpace(configCode.District.DistrictName) ? null : configCode.District.DistrictName
            };
        }

        public static explicit operator ConfigCodeDto(ConfigCode configCode)
        {
            if (configCode == null) return null;

            return new ConfigCodeDto
            {
                Active = configCode.Active,
                ConfigCodeAnnotation = configCode.ConfigCodeAnnotation,
                ConfigCodeID = configCode.ConfigCodeID,
                ConfigCodeName = configCode.ConfigCodeName,
                Created = configCode.Created,
                CreatedBy = new UserDto { UserID = configCode.CreatedByUserId },
                District = new DistrictDto { DistrictId = configCode.DistrictId }
            };
        }
    }
}
