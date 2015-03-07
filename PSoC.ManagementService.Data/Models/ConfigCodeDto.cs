﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using PSoC.ManagementService.Data.Interfaces;
using PSoC.ManagementService.Security;

namespace PSoC.ManagementService.Data.Models
{
    public partial class ConfigCodeDto : IEntity
    {
        [Key]
        [Required]
        public Guid ConfigCodeID { get; set; }

        [Required]
        [StringLength(10)]
        public string ConfigCodeName { get; set; }

        public bool Active { get; set; }

        public DateTime Created { get; set; }

        [StringLength(80)]
        public string ConfigCodeAnnotation { get; set; }

        public UserDto CreatedBy { get; set; }

        public DistrictDto District { get; set; }
    }
}
