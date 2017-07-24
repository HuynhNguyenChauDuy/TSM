using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;
using TSM.Data.ModelViews;
using TSM.DataAccess;
using TSM.Models;
using TSM.Models.ManageViewModels;
using TSM.Services;
namespace TSM.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly MailKitService _mailKit;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly ILogger _logger;

        private readonly LeaveManager _leaveManager;
        private readonly IMapper _mapper;
        private IToastNotification _toastNotification;

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          ILoggerFactory loggerFactory,
          ApplicationDbContext context,
          LeaveManager leaveManager,
          IMapper mapper,
          IToastNotification toastNotification,
          MailKitService mailKit
          )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _context = context;
            _leaveManager = leaveManager;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _mailKit = mailKit;
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var model = new IndexViewModel
            {
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        // Get: /Manage/TimeSheetManager
        [HttpGet]
        public async Task<ActionResult> GetTimesheet()
        {
            var userID = (await GetCurrentUserAsync()).Id;
            var viewContext = new LeaveWrapper
            {
                LeaveVM = await _leaveManager.GetLeavebyUserIdsAsync(userID)
            };

            var user = await _context.Users
                .Include(item => item.Team)
                .Where(item => item.Id.CompareTo(userID) == 0)
                .FirstOrDefaultAsync();

            ViewData["userTeam"] = user.Team != null ? user.Team.TeamName : "";

            return View(viewContext);
        }

        [HttpGet]
        public async Task<ActionResult> GetTimesheetByState(Leave.eState state)
        {
            var userID = (await GetCurrentUserAsync()).Id;
            var viewContext = new LeaveWrapper
            {
                LeaveVM = await _leaveManager.GetLeavebyUserIdsAsync(userID, state)
            };

            var user = await _context.Users
                .Include(item => item.Team)
                .Where(item => item.Id.CompareTo(userID) == 0)
                .FirstOrDefaultAsync();

            ViewData["userTeam"] = user.Team != null ? user.Team.TeamName : "";

            return View("GetTimesheet", viewContext);
        }

        [HttpGet]
        [Authorize(Roles = ("Project Manager,Team Leader"))]
        public async Task<ActionResult> GetTimesheetManager(DateTime? date = null)
        {
            var user = await GetCurrentUserAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
           
            IEnumerable<LeaveVM> leaveVM = null;
            if (userRoles.Contains("Project Manager"))
            {
                leaveVM = await _leaveManager.GetAllLeavesAsync(date);
            }
            else
            {
                leaveVM = await _leaveManager.GetLeavesByTeamIdAsync(user.TeamID, date);
            }

            var viewContext = new LeaveWrapper
            {
                LeaveVM = leaveVM
            };

            ViewData["curDate"] = date != null ? date.Value.ToString("dd/MM/yyyy") : DateTime.Today.ToString("dd/MM/yyyy"); 
            return View(viewContext);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveSubmit(LeaveWrapper submit)
        {
            // toast message properties
            string messageTitle;
            string message;
            ToastEnums.ToastType messageType;

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                Leave newLeave = _mapper.Map<LeaveFormVM, Leave>(submit.LeaveFormVM);

                var result = await _leaveManager.SubmitLeaveAsync(newLeave, user);

                // assign toast message properties
                if (result != null)
                {
                    messageTitle = "Sucessful";
                    message = "Your request submitted";
                    messageType = ToastEnums.ToastType.Success;

                    var leaveType = (_context.LeaveTypes.Find(newLeave.LeaveTypeID).LeaveName);

                    // email setup
                    MimeMessage email = _mailKit.SetUpEmailInfo(user.Email, null, "Your leave request is being handled");
                    email.Body = await _mailKit.SetUpEmailBody_LeaveSubmit(newLeave.ID);

                    _mailKit.Send(email);
                }
                else
                {
                    messageTitle = "Error";
                    message = "The range of dates already existed";
                    messageType = ToastEnums.ToastType.Error;
                }
            }
            else
            {
                messageTitle = "Validation error";
                message = "Please check your leave form again";
                messageType = ToastEnums.ToastType.Error;
            }

            _toastNotification.AddToastMessage(
              messageTitle, message, messageType);

            return RedirectToAction(nameof(GetTimesheet));
        }

        [HttpPost]
        public async Task<IActionResult> EditLeave(LeaveWrapper submit)
        {
            // toast message properties
                string messageTitle;
                string message;
                ToastEnums.ToastType messageType;

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                Leave changes = _mapper.Map<LeaveFormVM, Leave>(submit.LeaveFormVM);

                bool result = await _leaveManager.EditLeaveAsync(changes, user.Id);

                // assign toast message properties
                if (result)
                {
                    messageTitle = "Sucessful";
                    message = "Your request edited";
                    messageType = ToastEnums.ToastType.Success;

                    // email setup
                    MimeMessage email = _mailKit.SetUpEmailInfo(user.Email, null, "Your leave request is being handled");
                    email.Body = await _mailKit.SetUpEmailBody_EditLeave(changes.ID);

                    _mailKit.Send(email);
                }
                else
                {
                    messageTitle = "Error";
                    message = "Unable to edit your request";
                    messageType = ToastEnums.ToastType.Error;
                }
            }
            else
            {
                messageTitle = "Validation errors";
                message = "Please check your request again";
                messageType = ToastEnums.ToastType.Error;
            }

            _toastNotification.AddToastMessage(
              messageTitle, message, messageType);

            return RedirectToAction(nameof(GetTimesheet));
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaveDetailForManager(string leaveId)
        {
            LeaveVM leaveVM = await _leaveManager.GetLeaveDetailForManager(leaveId);
            return Json(leaveVM);
        }

        public async Task<IActionResult> GetLeaveDetail(string leaveId)
        {
            LeaveVM leaveVM = await _leaveManager.GetLeaveDetail(leaveId);
            return Json(leaveVM);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLeave(string leaveID)
        {
            string messageTitle = "Error";
            string message = "Unable to delete your request";
            ToastEnums.ToastType messageType = ToastEnums.ToastType.Error;

            bool result = false;

            if (leaveID != null && leaveID != "")
            {
                var user = await GetCurrentUserAsync();

                var leave = await _context.Leaves.Include(item => item.User).Include(item => item.LeaveType)
                    .Where(item => item.ID.CompareTo(leaveID) == 0).FirstOrDefaultAsync();

                result = await _leaveManager.DeleteLeaveAsync((string)leaveID, user.Id);

                if (result)
                {
                    messageTitle = "Successful";
                    message = "Your request deleted";
                    messageType = ToastEnums.ToastType.Success;

                    // email setup
                    MimeMessage email = _mailKit.SetUpEmailInfo(user.Email, null, "Your leave request was deleted");
                    email.Body = await _mailKit.SetUpEmailBody_DeleteLeave(leave);

                    _mailKit.Send(email);
                }
            }

            _toastNotification.AddToastMessage(messageTitle, message, messageType);

            return RedirectToAction(nameof(GetTimesheet));
        }

        [HttpGet]
        public async Task<IActionResult> GetCCRecommend(string query)
        {
            var teamID = (await GetCurrentUserAsync()).TeamID;
            var team = await _context.Teams.Include(item => item.Users).Where(item => item.ID.CompareTo(teamID) == 0).FirstOrDefaultAsync();

            var emailList = new List<CCVM>();

            if(team != null)
            {
                foreach(var item in team.Users)
                {
                    if (item.Email.Contains(query))
                    {
                        emailList.Add(new CCVM { Id = item.Id, Email = item.Email });
                    }
                }
            }

            return Json(emailList);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        [HttpPost]
        [Authorize(Roles = ("Project Manager,Team Leader"))]
        public async Task<IActionResult> HandleSingleRequest(LeaveWrapper request)
        {
            // toast message properties
            string messageTitle;
            string message;
            ToastEnums.ToastType messageType;

            var approverId = (await GetCurrentUserAsync()).Id;
            var result = await _leaveManager.HandleSingleRequestAysnc(request.LeaveHandleVM, approverId);

            // assign message properties
            if(result)
            {
                message = request.LeaveHandleVM.Result == Leave.eState.Approved ? 
                    "Request approved" : "Request rejected";

                messageTitle = "Successful";
                messageType = ToastEnums.ToastType.Success;

                var employeeEmail = (await _context.Leaves
                                        .Include(item => item.User)
                                        .Where(item => item.ID.CompareTo(request.LeaveHandleVM.LeaveID) == 0)
                                        .FirstOrDefaultAsync()).User.Email;

                // email setup
                MimeMessage email = _mailKit.SetUpEmailInfo(employeeEmail, null, "Your leave request handled");
                email.Body = await _mailKit.SetUpEmailBody_HandleRequest(request.LeaveHandleVM.LeaveID, request.LeaveHandleVM.Result);

                _mailKit.Send(email);
            }
            else
            {
                messageTitle = "Error";
                message = "Unable to handle this request";
                messageType = ToastEnums.ToastType.Error;
            }

            _toastNotification.AddToastMessage(
            messageTitle, message, messageType);

            return RedirectToAction(nameof(GetTimesheetManager), new { date = request.LeaveHandleVM.CurDate });
        }

        [HttpPost]
        [Authorize(Roles = ("Project Manager,Team Leader"))]
        public async Task<IActionResult> HandleMultipleRequests(LeaveWrapper request)
        {
            // toast message properties
            string messageTitle;
            string message;
            ToastEnums.ToastType messageType;

            var userId = (await GetCurrentUserAsync()).Id;
            List<string> result = await _leaveManager.HandleMultipleRequestsAsync(request.LeaveHandleVM_Multiple, userId);

            // assign message properties
            if(result != null)
            {
                message = request.LeaveHandleVM_Multiple.Result == Leave.eState.Approved ?
                    "Requests approved" : "Requests rejected";

                messageTitle = "Successful";
                messageType = ToastEnums.ToastType.Success;

                // send email

                await _mailKit.SendMultipleEmail(result, request.LeaveHandleVM_Multiple.Result);
            }
            else
            {
                messageTitle = "Error";
                message = "Unable to handle these requests";
                messageType = ToastEnums.ToastType.Error;
            }

            _toastNotification.AddToastMessage(
           messageTitle, message, messageType);

            return RedirectToAction(nameof(GetTimesheetManager), new { date = request.LeaveHandleVM_Multiple.CurDate });

        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        
        // GET: /Manage/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
          //  await _smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(1, "User enabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            // Send an SMS to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }
            // If we got this far, something failed, redisplay the form
            ModelState.AddModelError(string.Empty, "Failed to verify phone number");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = ManageMessageId.Error;
            if (result.Succeeded)
            {
                message = ManageMessageId.AddLoginSuccess;
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }


        #endregion
    }
}
