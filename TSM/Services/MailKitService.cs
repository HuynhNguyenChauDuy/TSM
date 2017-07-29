using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;
using TSM.Models;


namespace TSM.Services
{
    public class MailKitService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _contextAccessor;

        public MailKitService(ApplicationDbContext ctx, IHostingEnvironment env, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _context = ctx;
            _env = env;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public void Send(MimeMessage Email)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate("hremailtesting@gmail.com", "MatKhau1@3");
                    client.Send(Email);
                    client.Disconnect(true);
                }
            }
            catch
            {
                return;
            }
        }

        public MimeMessage SetUpEmailInfo(string ToEmail, List<string> ccEmails, string subject)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("TSM", "hremailtesting@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", ToEmail));
            
            if(ccEmails != null)
            {
                foreach(var email in ccEmails)
                {
                    emailMessage.Cc.Add(new MailboxAddress("", email));
                }
            }

            emailMessage.Subject = subject;

            return emailMessage;
        }

        public async Task<TextPart> Setup_EmailBody_ReplaceKeyword(string template, Leave leave)
        {
            try
            {
                template = template.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                template = template.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                template = template.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                template = template.Replace("WORKSHIFT", leave.WorkShift.ToString());
                template = template.Replace("NOTE", System.Net.WebUtility.HtmlDecode(leave.Note));

                return new TextPart("html")
                {
                    Text = template
                };

            }
            catch
            {
                return null;
            }
        }

        public async Task<List<string>> CcIdToEmailAsync(string CC)
        {
            try
            {
                var emailList = new List<string>();
                if (CC != "" && CC != null)
                {
                    var cc = CC.Split(new string[] { "," }, StringSplitOptions.None);
                    foreach (var item in cc)
                    {
                        var userEmail = (await _context.Users.Where(m => m.Id == item).FirstOrDefaultAsync()).Email;
                        if (userEmail != null)
                        {
                            emailList.Add(userEmail);
                        }
                    }
                }
                return emailList;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<bool> SendEmail_LeaveSubmitAsync(string leaveId)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveId);
                var user = await _context.Users.FindAsync(leave.ApplicationUserID);
                var userRoles =  await _userManager.GetRolesAsync(user);
                var ccEmailList = await CcIdToEmailAsync(leave.CCId);
                var receiverName = "";

                MimeMessage email = null;
                if (userRoles.Contains("Project Manager"))
                {
                    await SendEmail_PMLeaveSubmitAsync(leaveId);
                }
                else if(userRoles.Contains("Team Leader"))
                {
                    var projectManagerRole = (await _context.Roles.Include(item => item.Users)
                                             .Where(item => item.Name == "Project Manager")
                                            .FirstOrDefaultAsync()).Users.FirstOrDefault();
                
                    var projectManager = await _context.Users.FindAsync(projectManagerRole.UserId);
                    email = SetUpEmailInfo(projectManager.Email, null, "Team Leader Leave Request");

                    receiverName = projectManager.UserName;
                    email.Body = await SetUpEmailBody_LeaveSubmitAsync(leaveId, receiverName);

                    Send(email);
                }
                else 
                {
                    ApplicationUser teamLeader = null;
                    foreach(var item in _context.Users.Where(item => item.TeamID == user.TeamID))
                    {
                        var roles = await _userManager.GetRolesAsync(item);
                        if(roles.Contains("Team Leader"))
                        {
                            teamLeader = item;
                            break;
                        }
                    }

                    if (teamLeader != null)
                    {
                        email = SetUpEmailInfo(teamLeader.Email, null, "DEV Leave Request");
                        receiverName = teamLeader.UserName;
                       
                        email.Body = await SetUpEmailBody_LeaveSubmitAsync(leaveId, receiverName);

                        Send(email);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendEmail_PMLeaveSubmitAsync(string leaveID)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveID);
                var ccEmails = await CcIdToEmailAsync(leave.CCId);

                var email = SetUpEmailInfo(leave.User.Email, ccEmails, "PM Leave Announcement");

                var emailTemplate = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leaverequestPM.html");
              
                email.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplate, leave);

                Send(email);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TextPart> SetUpEmailBody_LeaveSubmitAsync(string leaveId, string receiverName)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveId);
                var userID = (await _context.Leaves.FindAsync(leaveId)).ApplicationUserID;
                var userName = (await _context.Users.FindAsync(userID)).UserName;
                
                var request = _contextAccessor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme,"://",request.Host.ToUriComponent(),"/Request/",leaveId);
               
                string emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leaverequest.html");

                emailTemp = emailTemp.Replace("RECEIVER", receiverName);
                emailTemp = emailTemp.Replace("SENDER", userName);

                emailTemp = emailTemp.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                emailTemp = emailTemp.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                emailTemp = emailTemp.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                emailTemp = emailTemp.Replace("WORKSHIFT", leave.WorkShift.ToString());
                emailTemp = emailTemp.Replace("NOTE", System.Net.WebUtility.HtmlDecode(leave.Note));
                emailTemp = emailTemp.Replace("LINK", absoluteUri);

                var tpart = new TextPart("html")
                {
                    Text = emailTemp
                };

                return tpart;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SendEmail_RequestHandledAsync(string leaveId)
        {
            try
            { 
                // set up email for request owner
                var leave = await GetLeavebyIDAsync(leaveId);
                var result = leave.State;
                var approver = await _context.Users.FindAsync(leave.ApproverID);

                MimeMessage ownerEmail = null;
                if(result == Leave.eState.Approved)
                {
                    // include CC
                    var ccEmails = await CcIdToEmailAsync(leave.CCId);
                    var emailTitle = leave.User.UserName + "'s Leave Request Handled";
                    ownerEmail = SetUpEmailInfo(leave.User.Email, ccEmails, emailTitle);
                }
                else
                {
                    // Not include CC
                    ownerEmail = SetUpEmailInfo(leave.User.Email, null, "Your Leave Request Handled");
                }

                string emailTemplateForOwner = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leaveresult.html");

                emailTemplateForOwner = emailTemplateForOwner.Replace("RECEIVER", leave.User.UserName);
                emailTemplateForOwner = emailTemplateForOwner.Replace("RESULT", result.ToString());
                emailTemplateForOwner = emailTemplateForOwner.Replace("APPROVEDDATE", leave.ApprovedDate.ToString("dd/MM/yyyy"));
                emailTemplateForOwner = emailTemplateForOwner.Replace("APPROVER", approver.UserName);

                ownerEmail.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForOwner, leave);

                // set up email for approver
                var approverEmail = SetUpEmailInfo(approver.Email, null, "You Handled A Leave Request");

                string emailTemplateForApprover = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavehandledapprover.html");

                emailTemplateForApprover = emailTemplateForApprover.Replace("RECEIVER", approver.UserName);
                emailTemplateForApprover = emailTemplateForApprover.Replace("SENDER", leave.User.UserName);
                emailTemplateForApprover = emailTemplateForApprover.Replace("RESULT", result.ToString());
                emailTemplateForApprover = emailTemplateForApprover.Replace("APPROVEDDATE", leave.ApprovedDate.ToString("dd/MM/yyyy"));

                approverEmail.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForApprover, leave);

                Send(ownerEmail);
                Send(approverEmail);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendEmail_DeleteRequestAsync(Leave leave)
        {
            try
            {
                // set up email for request owner
                var ownerEmail = SetUpEmailInfo(leave.User.Email, null, "Your Deleted Leave Request");
                string emailTemplateForOwner = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavedeleted.html");
                emailTemplateForOwner = emailTemplateForOwner.Replace("RECEIVER", leave.User.UserName);
                ownerEmail.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForOwner, leave);

                Send(ownerEmail);

                // set up email for approver
                var approverName = "";
                var aproverEmail = "";
                var emailTitle = "";

                var userRoles = await _userManager.GetRolesAsync(leave.User);
                if (userRoles.Contains("Project Manager"))
                {
                    Send(ownerEmail);
                    return true;
                }
                else if (userRoles.Contains("Team Leader"))
                {
                    var projectManagerID = (await _context.Roles.Include(item => item.Users)
                                             .Where(item => item.Name == "Project Manager")
                                            .FirstOrDefaultAsync()).Users.FirstOrDefault();

                    var projectManager = await _context.Users.FindAsync(projectManagerID.UserId);

                    approverName = projectManager.UserName;
                    aproverEmail = projectManager.Email;
                    emailTitle = "Team Leader Leave Deleted";
                }
                else
                {
                    foreach (var item in _context.Users.Where(item => item.TeamID == leave.User.TeamID))
                    {
                        var roles = await _userManager.GetRolesAsync(item);
                        if (roles.Contains("Team Leader"))
                        {
                            aproverEmail = item.Email;
                            approverName = item.UserName;
                            emailTitle = "DEV Leave Request Deleted";
                            break;
                        }
                    }
                }

                if (approverName != "" && aproverEmail != "")
                {
                    var mailForApprover = SetUpEmailInfo(aproverEmail, null, emailTitle);

                    var emailTemplateForApprover = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavedeletedForApprover.html");
                    emailTemplateForApprover = emailTemplateForApprover.Replace("RECEIVER", approverName);
                    emailTemplateForApprover = emailTemplateForApprover.Replace("SENDER", leave.User.UserName);

                    mailForApprover.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForApprover, leave);

                    Send(mailForApprover);
                }
                    
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendEmail_UpdateRequestAsync(string leaveId)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveId);
                // set up email for request owner
                var ownerEmail = SetUpEmailInfo(leave.User.Email, null, "Your Updated Leave Request");
                string emailTemplateForOwner = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leaveupdated.html");
                emailTemplateForOwner = emailTemplateForOwner.Replace("RECEIVER", leave.User.UserName);
                ownerEmail.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForOwner, leave);

                Send(ownerEmail);

                // set up email for approver
                var approverName = "";
                var aproverEmail = "";
                var emailTitle = "";

                var userRoles = await _userManager.GetRolesAsync(leave.User);
                if (userRoles.Contains("Project Manager"))
                {
                    Send(ownerEmail);
                    return true;
                }
                else if (userRoles.Contains("Team Leader"))
                {
                    var projectManagerID = (await _context.Roles.Include(item => item.Users)
                                             .Where(item => item.Name == "Project Manager")
                                            .FirstOrDefaultAsync()).Users.FirstOrDefault();

                    var projectManager = await _context.Users.FindAsync(projectManagerID.UserId);

                    approverName = projectManager.UserName;
                    aproverEmail = projectManager.Email;
                    emailTitle = "Team Leader Leave Updated";
                }
                else
                {
                    foreach (var item in _context.Users.Where(item => item.TeamID == leave.User.TeamID))
                    {
                        var roles = await _userManager.GetRolesAsync(item);
                        if (roles.Contains("Team Leader"))
                        {
                            aproverEmail = item.Email;
                            approverName = item.UserName;
                            emailTitle = "DEV Leave Request Updated";
                            break;
                        }
                    }
                }

                if (approverName != "" && aproverEmail != "")
                {
                    var mailForApprover = SetUpEmailInfo(aproverEmail, null, emailTitle);

                    var emailTemplateForApprover = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leaveupdateforapprover.html");
                    emailTemplateForApprover = emailTemplateForApprover.Replace("RECEIVER", approverName);
                    emailTemplateForApprover = emailTemplateForApprover.Replace("SENDER", leave.User.UserName);

                    var request = _contextAccessor.HttpContext.Request;
                    var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/Request/", leaveId);

                    emailTemplateForApprover = emailTemplateForApprover.Replace("LINK", absoluteUri);

                    mailForApprover.Body = await Setup_EmailBody_ReplaceKeyword(emailTemplateForApprover, leave);

                    Send(mailForApprover);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Leave> GetLeavebyIDAsync(string leaveId)
        {
            try
            {
                return await _context.Leaves
                    .Include(item => item.User)
                    .Include(item => item.LeaveType)
                    .Where(item => item.ID == leaveId).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SendEmail_MultippleRequestHandledAsync(List<string> leaveIdList)
        {
            try
            {
                if(leaveIdList != null)
                {
                    foreach(var item in leaveIdList)
                    {
                       await SendEmail_RequestHandledAsync(item);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
