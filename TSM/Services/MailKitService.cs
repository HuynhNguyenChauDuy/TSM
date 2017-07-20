using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;

namespace TSM.Services
{
    public class MailKitService
    {
        private readonly ApplicationDbContext _context;
        private IHostingEnvironment _env;

        public MailKitService(ApplicationDbContext ctx, IHostingEnvironment env)
        {
            _context = ctx;
            _env = env;
        }

        public void Send(MimeMessage Email)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // chuyển về file XML?
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

        public  MimeMessage SetUpEmailInfo(string ToEmail, List<string> ccEmails, string subject)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("TSM", "hremailtesting@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", ToEmail));
            emailMessage.Subject = subject;

            //// for CC
            //if(ccEmails != null && ccEmails.Count != 0)
            //{
               
            //}

            return emailMessage;
        }

        public async Task<TextPart> SetUpEmailBody_LeaveSubmit(string leaveId)
        {
            try
            {
                var leave = await GetLeavebyID(leaveId);

                string emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/leavesubmit.html");

                emailTemp = ReplaceTemplateByKeyWords(emailTemp,leave.FromDate,leave.ToDate,leave.User.UserName,leave.LeaveType.LeaveName,leave.WorkShift);

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

        public async Task<TextPart> SetUpEmailBody_DeleteLeave(Leave leave)
        {
            try
            {
                string emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/deletedleave.html");

                emailTemp = ReplaceTemplateByKeyWords(emailTemp, leave.FromDate, leave.ToDate, leave.User.UserName, leave.LeaveType.LeaveName, leave.WorkShift);

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

        public async Task<TextPart> SetUpEmailBody_EditLeave(string leaveId)
        {
            try
            {
                var leave = await GetLeavebyID(leaveId);

                string emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/editleave.html");

                emailTemp = ReplaceTemplateByKeyWords(emailTemp, leave.FromDate, leave.ToDate, leave.User.UserName, leave.LeaveType.LeaveName, leave.WorkShift);

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

        public async Task<bool> SendMultipleEmail(List<string> leaveIdList, Leave.eState result)
        {
            try
            {
                foreach(var item in leaveIdList)
                {
                    var userEmail = (await _context.Leaves
                                            .Include(item2 => item2.User)
                                            .Where(item2 => item2.ID.CompareTo(item) == 0)
                                            .FirstOrDefaultAsync()).User.Email;


                    var email = this.SetUpEmailInfo(userEmail ,null, "Your leave request handled");
                    email.Body = await this.SetUpEmailBody_HandleRequest(item, result);

                    this.Send(email);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TextPart> SetUpEmailBody_HandleRequest(string leaveId, Leave.eState result)
        {
            try
            {
                var leave = await GetLeavebyID(leaveId);

                var approver = (await _context.Users.FindAsync(leave.ApproverID)).UserName;

                string emailTemp = "";

                if(result == Leave.eState.Approved)
                {
                    emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/approvedrequest.html");
                }
                else
                {
                    emailTemp = System.IO.File.ReadAllText(_env.WebRootPath + "/emailTemplate/rejectedrequest.html");
                }

                emailTemp = ReplaceTemplateByKeyWords(emailTemp, leave.FromDate, leave.ToDate, leave.User.UserName, leave.LeaveType.LeaveName, leave.WorkShift);

                emailTemp = emailTemp.Replace("APPROVER", approver);
                emailTemp = emailTemp.Replace("APPROVEDTIME", leave.ApprovedDate.ToString("dd/MM/yyyy hh:mm:ss tt"));

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

        public async Task<Leave> GetLeavebyID(string leaveId)
        {
            try
            {
                return await _context.Leaves
                    .Include(item => item.User)
                    .Include(item => item.LeaveType)
                    .Where(item => item.ID.CompareTo(leaveId) == 0).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public string ReplaceTemplateByKeyWords(string emailTemp, DateTime fromDate, DateTime toDate, string userName, string leaveName, Leave.eWorkShift workShift)
        {
            try
            {
                var result = emailTemp.Replace("USERNAME",userName);
                result = result.Replace("LEAVETYPE", leaveName);
                result = result.Replace("WORKSHIFT", workShift.ToString());

                string DateRange = "FROM " + fromDate.ToString("dd-MM-yyyy") + " TO " + toDate.ToString("dd-MM-yyyy");
                result = result.Replace("DATEOFF", DateRange);

                return result;
            }
            catch
            {
                return null;
            }
        }

    }
}
