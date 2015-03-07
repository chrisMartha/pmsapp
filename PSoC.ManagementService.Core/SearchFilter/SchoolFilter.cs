using System;
using System.Collections.Generic;

using PSoC.ManagementService.Core.Extensions;

namespace PSoC.ManagementService.Core.SearchFilter
{
    /// <summary>
    /// Represents School DropDown Filter on Global, District Admin dashboard  
    /// </summary>
    public class SchoolFilter : SearchFilter
    {
        /// <summary>
        /// Overloaded constructor to access List of Guid values for School Filter
        /// </summary>
        /// <param name="filterValues"></param>
        public SchoolFilter(IList<Guid> filterValues)
            : base(FilterType.SchoolId)
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
