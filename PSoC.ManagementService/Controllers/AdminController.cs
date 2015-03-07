using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

using PSoC.ManagementService.Core;
using PSoC.ManagementService.Core.Extensions;
using PSoC.ManagementService.Core.SearchFilter;
using PSoC.ManagementService.Filter;
using PSoC.ManagementService.Models;
using PSoC.ManagementService.Responses;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Controllers
{
    /// <summary>
    /// Admin Controller - Renders Admin Landing Page with Dashboard
    /// </summary>
    [System.Web.Mvc.Authorize(Roles = "GlobalAdmin, DistrictAdmin, SchoolAdmin")]
    public class AdminController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IDistrictService _districtService;
        private readonly ISchoolService _schoolService;
        private readonly IAccessPointService _accessPointService;
        private AdminAccessPointViewModel _viewModel;

        #region UserData

        private bool IsAuthenticated
        {
            get { return (User != null && User.Identity != null && User.Identity.IsAuthenticated); }
        }

        private ClaimsIdentity UserIdentity
        {
            get
            {
                if (IsAuthenticated) return (ClaimsIdentity) User.Identity;
                var ex = new Exception("User not authenticated or failed to retrieve identity");
                PEMSEventSource.Log.ApplicationException(ex.Message);
                throw ex;
            }
        }

        private AdminType UserType
        {
            get
            {
                var role = UserIdentity.FindFirst(ClaimTypes.Role);
                if (role != null && !string.IsNullOrEmpty(role.Value)) return Enum<AdminType>.Parse(role.Value);
                var ex = new Exception("User Role not found");
                PEMSEventSource.Log.ApplicationException(ex.Message);
                throw ex;
            }
        }

        private string Username
        {
            get
            {
                var username = UserIdentity.FindFirst(ClaimTypes.Name);
                if (username != null && !string.IsNullOrEmpty(username.Value)) return username.Value;
                var ex = new Exception("Username not found");
                PEMSEventSource.Log.ApplicationException(ex.Message);
                throw ex;
            }
        }

        private Guid DistrictId
        {
            get
            {
                Guid districtId;
                var district = UserIdentity.FindFirst(x => x.Type == "District");
                if ((district == null) || (!Guid.TryParse(district.Value, out districtId)))
                {
                    var ex = new Exception("District Id not found");
                    PEMSEventSource.Log.ApplicationException(ex.Message);
                    throw ex;
                }
                return districtId;
            }
        }

        private Guid SchoolId
        {
            get
            {
                Guid schoolId;
                var school = UserIdentity.FindFirst(x => x.Type == "School");
                if ((school == null) || (!Guid.TryParse(school.Value, out schoolId)))
                {
                    var ex = new Exception("School Id not found");
                    PEMSEventSource.Log.ApplicationException(ex.Message);
                    throw ex;
                }
                return schoolId;
            }
        }

        private Guid? InstitutionEntityId
        {
            get
            {
                Guid? institutionEntityId = null;
                switch (UserType)
                {
                    case AdminType.DistrictAdmin:
                        institutionEntityId = DistrictId;
                        break;
                    case AdminType.SchoolAdmin:
                        institutionEntityId = SchoolId;
                        break;
                }
                return institutionEntityId;
            }
        }

        #endregion

        /// <summary>
        /// Constructor of Admin dashboard controller
        /// </summary>
        /// <param name="deviceService">Device Service</param>
        /// <param name="districtService">District Service</param>
        /// <param name="schoolService">School Service</param>
        /// <param name="accessPointService">Access Point Service</param>
        public AdminController(IDeviceService deviceService,
                               IDistrictService districtService,
                               ISchoolService schoolService,
                               IAccessPointService accessPointService)
        {
            _deviceService = deviceService;
            _districtService = districtService;
            _schoolService = schoolService;
            _accessPointService = accessPointService;
        }

        /// <summary>
        /// Index page for Admin Dashboard
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.IsAuthenticated = IsAuthenticated;
            ViewBag.Username = Username;
            ViewBag.UserType = UserType.ToString();
            await LoadFiltersAndInitViewModel().ConfigureAwait(false);
            return View(_viewModel);
        }

        /// <summary>
        /// Invoked as AJAX call
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [AjaxRequest]
        public async Task<ActionResult> Dashboard(DataTablePageRequestModel dataTable)
        {
            /* TODO Override HTTPResponse Status as 401 in case of session time out. 
               Currently, default is returned as 200 and 401 is returned as part of response header X-Responded-JSON 
            */

            if (dataTable == null)
            {
                dataTable = new DataTablePageRequestModel();
            }

            var filters = ExtractSearchFilters(dataTable);
            var result = await _deviceService.GetAccessPointDeviceStatusAsync(
                                    UserType,
                                    InstitutionEntityId,
                                    dataTable.Length,
                                    dataTable.Start,
                                    filters,
                                    null).ConfigureAwait(false);

            return Json(new DataTablesResponse<AccessPointDeviceStatus>(
                                    result.Item1,
                                    result.Item2,
                                    result.Item2,
                                    dataTable.Draw));
        }

        /// <summary>
        /// Trigger when and only when district list selection change
        /// </summary>
        /// <param name="value">
        /// value: {
        ///     District:[],
        ///     School:[]
        /// }
        /// </param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        [AjaxRequest]
        public ActionResult GetSchoolsByDistricts([FromBody] DistrictDropdownPostModel value)
        {
            List<string> districtSelected = (value != null && value.District != null)
                ? value.District
                : new List<string>();
            List<string> schoolSelected = (value != null && value.School != null)
                ? value.School
                : new List<string>();

            var result = new List<SchoolDropdownViewModel>();
            var schoolAll = TempData["schoolList"] as List<Tuple<Guid, string, Guid>>;
            if (schoolAll != null)
            {
                if (districtSelected.Count == 0)
                {
                    result.AddRange(
                        from school in schoolAll
                        select new SchoolDropdownViewModel
                        {
                            SchoolId = school.Item1,
                            SchoolName = school.Item2,
                            Checked = false
                        });
                }
                else
                {
                    result.AddRange(
                        from school in schoolAll
                        where districtSelected.Contains(school.Item3.ToString())
                        select new SchoolDropdownViewModel
                        {
                            SchoolId = school.Item1,
                            SchoolName = school.Item2,
                            Checked = schoolSelected.Contains(school.Item1.ToString())
                        });
                }
            }
            TempData["schoolList"] = schoolAll;
            return Json(result);
        }

        private IReadOnlyDictionary<FilterType, SearchFilter> ExtractSearchFilters(DataTablePageRequestModel data)
        {
            var filters = new Dictionary<FilterType, SearchFilter>();
            var districtWithSchoolList = new List<Guid>();
            if (data.SchoolFilter != null)
            {
                var guidList = new List<Guid>();
                foreach (var item in data.SchoolFilter)
                {
                    Guid guidId;
                    if (Guid.TryParse(item, out guidId))
                    {
                        guidList.Add(guidId);
                    }
                }
                filters.Add(FilterType.SchoolId, new SchoolFilter(guidList));

                var schoolAll = TempData["schoolList"] as List<Tuple<Guid, string, Guid>>;
                districtWithSchoolList.AddRange(
                    from school in schoolAll
                    where guidList.Contains(school.Item1)
                    select school.Item3);

                TempData["schoolList"] = schoolAll;
                }

            if (data.DistrictFilter != null)
            {
                var guidList = new List<Guid>();
                foreach (var item in data.DistrictFilter)
                {
                    Guid guidId;
                    if (Guid.TryParse(item, out guidId))
                    {
                        if (!districtWithSchoolList.Contains(guidId))
                        {
                            guidList.Add(guidId);
                        }
                    }
                }
                filters.Add(FilterType.DistrictId, new DistrictFilter(guidList));
            }
            return filters;
        }

        /// <summary>
        /// Load District, School and Accesspoint Filters DropDown For Admin Dashboard
        /// </summary>
        /// <returns></returns>
        private async Task LoadFiltersAndInitViewModel()
        {
            var districtList = await GetDistrictList().ConfigureAwait(false);
            var schoolList = await GetSchoolList().ConfigureAwait(false);
            var accessPointList = await GetAccessPointList().ConfigureAwait(false);
            _viewModel = new AdminAccessPointViewModel(districtList, schoolList, accessPointList);
        }

        //TODO: Revisit TempData by In Role Cache if there's performance issue
        private async Task<List<Tuple<Guid, string>>> GetDistrictList()
        {
            var districts = new List<District>();
            if (UserType == AdminType.SchoolAdmin)
            {
                var school = await _schoolService.GetByIdAsync(SchoolId).ConfigureAwait(false);
                districts.Add(school.District);
            }
            else
            {
                districts = await _districtService.GetAsync(Username).ConfigureAwait(false) as List<District>;
            }

            var result = new List<Tuple<Guid, string>>();
            if (districts != null)
            {
                result.AddRange(districts.Select(
                    district => new Tuple<Guid, string>(district.DistrictId, district.DistrictName)));
            }
            TempData["districtList"] = result;
            return result;
        }

        //TODO: Revisit TempData by In Role Cache if there's performance issue
        private async Task<List<Tuple<Guid, string>>> GetSchoolList()
        {
            var schoolCache = new List<Tuple<Guid, string, Guid>>();
            var result = new List<Tuple<Guid, string>>();
            var schools = await _schoolService.GetAsync(Username).ConfigureAwait(false);
            if (schools != null)
            {
                foreach (var school in schools)
                {
                    result.Add(new Tuple<Guid, string>(school.SchoolId, school.SchoolName));
                    schoolCache.Add(school.District == null
                        ? new Tuple<Guid, string, Guid>(school.SchoolId, school.SchoolName, Guid.Empty)
                        : new Tuple<Guid, string, Guid>(school.SchoolId, school.SchoolName, school.District.DistrictId));
                }
            }
            TempData["schoolList"] = schoolCache;
            return result;
        }
                
        private async Task<List<Tuple<string, string>>> GetAccessPointList()
        {
            var result = new List<Tuple<string, string>>();
            var accesspoints = await _accessPointService.GetAsync(Username).ConfigureAwait(false);
            if (accesspoints != null)
            {
                result.AddRange(accesspoints.Select(
                    accessPoint => new Tuple<string, string>(accessPoint.WifiBSSId, accessPoint.WifiSSId)));
            }
            return result;
        }
    }
}