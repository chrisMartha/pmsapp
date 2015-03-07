using System;

namespace PSoC.ManagementService.Core.SearchFilter
{
    /// <summary>
    /// Represents Base Search Filter, in use by Admin Dashboard
    /// </summary>
    public abstract class SearchFilter
    {
        protected SearchFilter(FilterType filtertype)
        {
            if (!Enum.IsDefined(typeof (FilterType), filtertype))
            {
                throw new ArgumentException(String.Format(@"SearchFilter - Invalid Filter Type : {0}", filtertype));
            }
            FilterType = filtertype;
        }

        /// <summary>
        /// Denotes field (such as DistrictId, SchoolId etc.) to be searched
        /// </summary>
        public FilterType FilterType { get; private set; }

        public abstract bool IsEnabled { get; }
    }
}
