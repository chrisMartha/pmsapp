using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using PSoC.ManagementService.Core;
using PSoC.ManagementService.Models;
using PSoC.ManagementService.Services.Interfaces;
using PSoC.ManagementService.Services.Logging;
using PSoC.ManagementService.Services.Models;

namespace PSoC.ManagementService.Controllers
{
    /// <summary>
    /// Controller for web portal's login and logout functions
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IDistrictService _districtService;
        private readonly ISchoolService _schoolService;
        private readonly ISchoolnetService _schoolnetService;

        /// <summary>
        /// Creates controller for web portal's login and logout functions
        /// </summary>
        /// <param name="adminService">Business service managing administrators, or web portal's users</param>
        /// <param name="schoolnetService">Business service responsible for authenticating web portal's user with SchoolNet</param>
        /// <param name="districtService">Business service managing districts</param>
        /// <param name="schoolService">Business service managing schools</param>
        public AccountController(IAdminService adminService, ISchoolnetService schoolnetService,
            IDistrictService districtService, ISchoolService schoolService)
        {
            _adminService = adminService;
            _districtService = districtService;
            _schoolService = schoolService;
            _schoolnetService = schoolnetService;
        }

        IAuthenticationManager Authentication
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        /// <summary>
        /// Web portal's login function
        /// </summary>
        /// <returns>Login view</returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            var loginModel = new LoginModel();
            return View("Login", loginModel);
        }

        /// <summary>
        /// Web portal's login function
        /// </summary>
        /// <param name="model">Login mode</param>
        /// <returns>Login view</returns>
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Is the user super-user, i.e. someone who can add administrators without authenticating through SchoolNet?
                if (model.Username == "FrenchToastMafia" && model.Password == "Pe@rs0n!")
                {
                    // Is authentication bypass for superuser enabled?
                    bool authenticationBypassEnabled = GlobalAppSettings.GetBool(GlobalAppSettings.AuthenticationBypassEnabled);
                    if (authenticationBypassEnabled)
                    {
                        Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                        Claim[] claims = {
                            new Claim(ClaimTypes.Name, model.Username),
                            new Claim(ClaimTypes.Sid, Guid.NewGuid().ToString()),
                            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", model.Username),
                            new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", model.Username),
                            new Claim(ClaimTypes.Role, AdminType.GlobalAdmin.ToString())
                        };
                        ClaimsIdentity identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie, ClaimTypes.Name, ClaimTypes.Role);

                        AuthenticationProperties authenticationProperties = new AuthenticationProperties { IsPersistent = true };
                        Authentication.SignIn(authenticationProperties, identity);

                        return RedirectToAction("Create", "Admins");
                    }
                }
                
                Admin admin = await _adminService.GetByUsernameAsync(model.Username).ConfigureAwait(false);
                if (admin == null || !admin.Active)
                {
                    PEMSEventSource.Log.AccountLoginFailed(admin != null
                        ? string.Format("Not an active user {0}", admin.UserId)
                        : "Not a whitelisted user"); // Can't save sensitive data in model.Username
                    ModelState.AddModelError("LogOnError", "Unauthorized to access this site.");
                    return View("Login", model);
                }
                Guid userId = admin.UserId;

                string oAuthApplicationId = null;
                string oAuthClientId = null;
                string oAuthUrl = null;

                switch (admin.AdminType)
                {
                    case AdminType.GlobalAdmin:
                        oAuthUrl = GlobalAppSettings.GetString(GlobalAppSettings.OAuthUrl);
                        oAuthClientId = GlobalAppSettings.GetString(GlobalAppSettings.OAuthClientId);
                        oAuthApplicationId = GlobalAppSettings.GetString(GlobalAppSettings.OAuthApplicationId);
                        break;

                    case AdminType.DistrictAdmin:
                        if (admin.DistrictId.HasValue)
                        {
                            District district = await _districtService.GetByIdAsync(model.Username, admin.DistrictId.Value).ConfigureAwait(false);
                            if (district == null)
                            {
                                PEMSEventSource.Log.AccountLoginFailed(string.Format("Unable to retrieve district information. for user: {0}", userId));
                                ModelState.AddModelError("ApplicationError", "An unexpected critical error has occured. Please contact the site administrator.");
                                return View("Login", model);
                            }

                            oAuthUrl = district.OAuthUrl;
                            oAuthClientId = district.OAuthClientId;
                            oAuthApplicationId = district.OAuthApplicationId;
                        }
                        break;

                    case AdminType.SchoolAdmin:
                        if (admin.SchoolId.HasValue)
                        {
                            School school = await _schoolService.GetByIdAsync(model.Username, admin.SchoolId.Value).ConfigureAwait(false);
                            if (school == null)
                            {
                                PEMSEventSource.Log.AccountLoginFailed(string.Format("Unable to retrieve school information. for user: {0}", userId));
                                ModelState.AddModelError("ApplicationError", "An unexpected critical error has occured. Please contact the site administrator.");
                                return View("Login", model);
                            }

                            oAuthUrl = school.District.OAuthUrl;
                            oAuthClientId = school.District.OAuthClientId;
                            oAuthApplicationId = school.District.OAuthApplicationId;
                        }
                        break;
                }

                if (!string.IsNullOrWhiteSpace(oAuthUrl) && !string.IsNullOrWhiteSpace(oAuthClientId) &&
                    !string.IsNullOrWhiteSpace(oAuthApplicationId))
                {
                    bool isAuthorized = await _schoolnetService.IsAuthorizedAsync(oAuthUrl, oAuthClientId, oAuthApplicationId,
                        model.Username, model.Password).ConfigureAwait(false);

                    if (isAuthorized)
                    {
                        Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        await _adminService.UpdateLastLoginDateTime(userId, DateTime.UtcNow).ConfigureAwait(false);
                        ClaimsIdentity identity = new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.Name, model.Username),
                                new Claim(ClaimTypes.Sid, userId.ToString()),
                                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", model.Username),
                                new Claim( "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", model.Username),
                                new Claim("District", admin.DistrictId.HasValue ? admin.DistrictId.Value.ToString() : string.Empty),
                                new Claim("School", admin.SchoolId.HasValue ? admin.SchoolId.Value.ToString() : string.Empty),
                                new Claim(ClaimTypes.Role, admin.AdminType.ToString())
                            },
                            DefaultAuthenticationTypes.ApplicationCookie,
                            ClaimTypes.Name, ClaimTypes.Role);

                        Authentication.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

                        return RedirectToAction("Index", "Admin");
                    }
                    
                    ModelState.AddModelError("LogOnError", "Failed to authorize with Schoolnet.");
                    PEMSEventSource.Log.AccountLoginFailed(string.Format("Failed to authorize with Schoolnet for user {0}", userId));
                    return View("Login", model);
                }
            }

            ModelState.AddModelError("LogOnError", "The user name or password provided is incorrect.");
            return View("Login", model);
        }

        /// <summary>
        /// Web portal's logout function
        /// </summary>
        /// <returns>Login view</returns>
        [HttpGet]
        public ActionResult Logout()
        {
            Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

    }
}