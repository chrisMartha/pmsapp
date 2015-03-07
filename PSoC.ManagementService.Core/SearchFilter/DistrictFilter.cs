using System;
using System.Collections.Generic;

using PSoC.ManagementService.Core.Extensions;

namespace PSoC.ManagementService.Core.SearchFilter
{
    /// <summary>
    /// Represents District DropDown Filter on Global Admin dashboard  
    /// </summary>
    public class DistrictFilter : SearchFilter
    {
        /// <summary>
        /// Overloaded constructor to access List of Guid values for District Filter
        /// </summary>
        /// <param name="filterValues"></param>
        public DistrictFilter(IList<Guid> filterValues) : base(FilterType.DistrictId)
        {
            IdValues = filterValues;
        }

        public IList<Guid> IdValues { get; private set; }

        public override sealed bool IsEnabled
        {
            get { return IdValues.HasElements(); }    
        }
     }
}
