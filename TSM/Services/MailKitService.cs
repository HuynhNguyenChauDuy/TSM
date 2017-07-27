using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
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
        private IHostingEnvironment _env;

        public MailKitService(ApplicationDbContext ctx, IHostingEnvironment env, UserManager<ApplicationUser> userManager )
        {
            _context = ctx;
            _env = env;
            _userManager = userManager;
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

        public async Task<List<string>> CCStringToEmailAsync(string CC)
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

        public async Task<bool> SendEmail_LeaveSubmitAsync(string leaveId, string ccEmail)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveId);
                var user = await _context.Users.FindAsync(leave.ApplicationUserID);
                var userRoles =  await _userManager.GetRolesAsync(user);
                var ccEmailList = await CCStringToEmailAsync(ccEmail);
                var receiverName = "";

                MimeMessage email = null;
                if (userRoles.Contains("Project Manager"))
                {
                    // send email to cc only
                    email = SetUpEmailInfo(user.Email, ccEmailList, "PM leave announcement");
                    receiverName = user.UserName;
                    email.Body = await SetUpEmailBody_LeaveSubmitAsync(leaveId, userRoles as List<string>, receiverName);

                    Send(email);
                }
                else if(userRoles.Contains("Team Leader"))
                {
                    var projectManagerRole = (await _context.Roles.Include(item => item.Users)
                                             .Where(item => item.Name == "Project Manager")
                                            .FirstOrDefaultAsync()).Users.FirstOrDefault();

                    var projectManager = await _context.Users.FindAsync(projectManagerRole.UserId);
                    email = SetUpEmailInfo(projectManager.Email, ccEmailList, "Team Leader Leave Request");

                    receiverName = projectManager.UserName;
                    email.Body = await SetUpEmailBody_LeaveSubmitAsync(leaveId, userRoles as List<string>, receiverName);

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
                        email = SetUpEmailInfo(teamLeader.Email, ccEmailList, "DEV Leave Request");
                        receiverName = teamLeader.UserName;
                        email.Body = await SetUpEmailBody_LeaveSubmitAsync(leaveId, userRoles as List<string>, receiverName);

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

        public async Task<TextPart> SetUpEmailBody_LeaveSubmitAsync(string leaveId, List<string> userRoles, string receiverName)
        {
            try
            {
                var leave = await GetLeavebyIDAsync(leaveId);
                var userID = (await _context.Leaves.FindAsync(leaveId)).ApplicationUserID;
                var userName = (await _context.Users.FindAsync(userID)).UserName;

                string emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/LeaveRequest.html");

                emailTemp = emailTemp.Replace("RECEIVER", receiverName);
                emailTemp = emailTemp.Replace("SENDER", userName);
                emailTemp = emailTemp.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                emailTemp = emailTemp.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                emailTemp = emailTemp.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                emailTemp = emailTemp.Replace("WORKSHIFT", leave.WorkShift.ToString());
                emailTemp = emailTemp.Replace("NOTE", System.Net.WebUtility.HtmlDecode(leave.Note));

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
                var leave = await GetLeavebyIDAsync(leaveId);
                var Approver = (await _context.Users.FindAsync(leave.ApproverID)).UserName;
                var result = leave.State;

                var email = SetUpEmailInfo(leave.User.Email, null, "Your Leave Request Handled");

                string emailTemplate = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavehandled.html");

                emailTemplate = emailTemplate.Replace("RECEIVER", leave.User.UserName);
                emailTemplate = emailTemplate.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                emailTemplate = emailTemplate.Replace("WORKSHIFT", leave.WorkShift.ToString());
                emailTemplate = emailTemplate.Replace("APPROVEDDATE", leave.ApprovedDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("APPROVER", Approver);
                emailTemplate = emailTemplate.Replace("RESULT", result.ToString());

                email.Body = new TextPart("html")
                {
                    Text = emailTemplate
                };

                Send(email);

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
                var email = SetUpEmailInfo(leave.User.Email, null, "Your Leave Request Deleted");

                string emailTemplate = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavedelete.html");

                emailTemplate = emailTemplate.Replace("RECEIVER",leave.User.UserName);
                emailTemplate = emailTemplate.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                emailTemplate = emailTemplate.Replace("WORKSHIFT", leave.WorkShift.ToString());

                email.Body = new TextPart("html")
                {
                    Text = emailTemplate
                };

                Send(email);

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
                var email = SetUpEmailInfo(leave.User.Email, null, "Your Leave Request Updated");

                string emailTemplate = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/updatleave.html");

                emailTemplate = emailTemplate.Replace("RECEIVER", leave.User.UserName);
                emailTemplate = emailTemplate.Replace("FROMDATE", leave.FromDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("TODATE", leave.ToDate.ToString("dd/MM/yyyy"));
                emailTemplate = emailTemplate.Replace("LEAVETYPE", leave.LeaveType.LeaveName);
                emailTemplate = emailTemplate.Replace("WORKSHIFT", leave.WorkShift.ToString());

                email.Body = new TextPart("html")
                {
                    Text = emailTemplate
                };

                Send(email);

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
