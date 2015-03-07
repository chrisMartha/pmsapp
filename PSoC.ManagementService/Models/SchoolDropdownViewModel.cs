using System;

namespace PSoC.ManagementService.Models
{
    public class SchoolDropdownViewModel
    {
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public bool Checked { get; set; }
    }
}