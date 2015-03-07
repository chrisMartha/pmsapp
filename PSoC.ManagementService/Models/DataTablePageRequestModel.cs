using System.Collections.Generic;

namespace PSoC.ManagementService.Models
{
    /// <summary>
    /// Class that encapsulates most parameters consumed by DataTables plugin
    /// </summary>
    public class DataTablePageRequestModel
    {
        /// <summary>
        /// Display start point/first record, in the current data set, to be shown
        /// </summary>
        public int Start 
        {
            get
            {
                if (_start < 0) { _start = 0; }
                return _start;
            }
            set { _start = value; } 
        }

        /// <summary>
        /// Number of records (page size) that the table can display in the current draw. It is expected that
        /// the number of recordsreturned will be equal to this number, unless the server has fewer records to return.
        /// </summary>
        public int Length
        {
            get
            {
                if (_length <= 0) { _length = 10; }
                return _length;
            }
            set { _length = value; }
        }

        /// <summary>
        /// Information for DataTables to use for rendering.
        /// </summary>
        public int Draw { get; set; }

        /// <summary>
        /// List of applied district filter. Null if district filter is not applied (none selected)
        /// </summary>
        public IEnumerable<string> DistrictFilter { get; set; }

        /// <summary>
        /// List of applied school filter. Null if school filter is not applied (none selected)
        /// </summary>
        public IEnumerable<string> SchoolFilter { get; set; }

        /// <summary>
        /// Default Settings
        /// </summary>
        public DataTablePageRequestModel()
        {
            _start = 0;   // Index (based on list before filtering) of the first Record on the page
            _length = 10; // Page Size (# of Records Per Page)
            DistrictFilter = null;
            SchoolFilter = null;
            Draw = 1;
        }

        private int _start;
        private int _length;
    }
}