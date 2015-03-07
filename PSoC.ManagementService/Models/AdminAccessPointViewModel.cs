using System;
using System.Collections.Generic;

namespace PSoC.ManagementService.Models
{
    /// <summary>
    /// View Model for Admin Dashboard
    /// </summary>
    public class AdminAccessPointViewModel
    {
        #region Variables

        private readonly List<Tuple<Guid, string>> _districtList;
        private readonly List<Tuple<Guid, string>> _schoolList;
        private readonly List<Tuple<string, string>> _accessPointList;

        #endregion Variables

        /// <summary>
        /// Constructor with districtList, schoolList, accessPointList
        /// </summary>
        /// <param name="districtList">District(id, name) list</param>
        /// <param name="schoolList">School(id, name) list</param>
        /// <param name="accessPointList">Wifi(BSSID, SSID) list</param>
        public AdminAccessPointViewModel(List<Tuple<Guid, string>> districtList, 
                                         List<Tuple<Guid, string>> schoolList,
                                         List<Tuple<string, string>> accessPointList)
        {
            _districtList = districtList;
            _schoolList = schoolList;
            _accessPointList = accessPointList;
        }

        /// <summary>
        /// Get District Dropdown List
        /// </summary>
        public IReadOnlyList<Tuple<Guid, string>> DistrictList
        {
            get { return _districtList; }
        }

        /// <summary>
        /// Get School Dropdown List
        /// </summary>
        public IReadOnlyList<Tuple<Guid, string>> SchoolList
        {
            get { return _schoolList; }
        }

        /// <summary>
        /// Get Access Point List
        /// </summary>
        public IReadOnlyList<Tuple<string, string>> AccessPointList
        {
            get { return _accessPointList; }
        }
    }
}