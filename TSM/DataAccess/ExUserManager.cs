using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;
using TSM.Data.ModelViews;
using TSM.Models;

namespace TSM.DataAccess
{
    public class ExUserManager
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public ExUserManager
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ProfileVM> GetUserDetailById(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(item => item.Team)
                    .Where(item => item.Id.CompareTo(userId) == 0).FirstOrDefaultAsync();

                var nSickleave = await CountLeaveByUserIdAsync(userId, "Sick Leave");
                var nAnnualLeave = await CountLeaveByUserIdAsync(userId, "Annual Leave");
                var nOtherLeave = await CountLeaveByUserIdAsync(userId, "Other");

                var nApprovedList = await CountLeavesByStateAsync(userId, Leave.eState.Approved);
                var nRejectedList = await CountLeavesByStateAsync(userId, Leave.eState.Rejected);
                var nWaitingList = await CountLeavesByStateAsync(userId, Leave.eState.OnQueue);

                var nApprovedDates = nSickleave + nAnnualLeave + nOtherLeave;
                var nRejectedDates = await CountRejectedDatesByUserIdAsync(userId);



                var userRoles = await _userManager.GetRolesAsync(user);

                var profileVM = new ProfileVM()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Position = userRoles != null ? userRoles[0] : "---------",
                    TeamName = user.Team != null ? user.Team.TeamName : "------",
                    DefaultAnnualLeave = user.DefaultAnnualLeave,
                    DefaultSickLeave = user.DefaultSickLeave,
                    NAnnualLeave = nAnnualLeave,
                    NOther = nOtherLeave,
                    NSickLeave = nSickleave,

                    NApprovedLeaves = nApprovedList,
                    NWaitingLeaves = nWaitingList,
                    NRejectedLeaves = nRejectedList,

                   NApprovedDates = nApprovedDates,
                   NRejectedDates = nRejectedDates
                };


                return profileVM;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<int> CountLeaveByUserIdAsync(string userId, string LeaveType)
        {
            try
            {
                int count = 0;
                foreach (var item in _context.Leaves.Include(item => item.LeaveType)
                                            .Where(item => item.ApplicationUserID.CompareTo(userId) == 0
                                                && item.LeaveType.LeaveName.CompareTo(LeaveType) == 0
                                                && item.State == Leave.eState.Approved))
                {
                    int days = (item.ToDate - item.FromDate).Days;
                    count += days == 0 ? 1 : (days + 1);
                }

                return count;
            }
            catch
            {
                return -1;
            }
        }

        private async Task<int> CountRejectedDatesByUserIdAsync(string userId)
        {
            try
            {
                int count = 0;
                foreach (var item in _context.Leaves
                       .Where(item => item.ApplicationUserID.CompareTo(userId) == 0
                       && item.State == Leave.eState.Rejected))
                {
                    int days = (item.ToDate - item.FromDate).Days;
                    count += days == 0 ? 1 : (days + 1);
                }

                return count;
            }
            catch
            {
                return -1;
            }
        }

        private async Task<int> CountLeavesByStateAsync(string userId, Leave.eState state)
        {
            try
            {
                return await _context.Leaves
                    .CountAsync(item => item.ApplicationUserID.CompareTo(userId) == 0 && item.State == state);
            }
            catch
            {
                return -1;
            }
        }

        public async Task<bool> EditProfileAsync(ProfileVM submit, string userId)
        {
            try
            {
                ApplicationUser user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return false;
                }

                user.Email = submit.Email;
                user.PhoneNumber = submit.PhoneNumber;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<String> GetSupervisorIdAsync(string userId)
        {
            try
            {
                string supervisorID = null;
                var user = await _context.Users.FindAsync(userId);

                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains("Project Manager"))
                {
                    supervisorID = user.Id;
                }
                else if (userRoles.Contains("Team Leader"))
                {
                    var projectManagerRole = (await _context.Roles.Include(item => item.Users)
                                             .Where(item => item.Name == "Project Manager")
                                            .FirstOrDefaultAsync()).Users.FirstOrDefault();

                    supervisorID = projectManagerRole.UserId;
                }
                else
                {
                    foreach (var item in _context.Users.Where(item => item.TeamID == user.TeamID))
                    {
                        var roles = await _userManager.GetRolesAsync(item);
                        if (roles.Contains("Team Leader"))
                        {
                            supervisorID = item.Id;
                            break;
                        }
                    }
                }

                return supervisorID;
            }
            catch
            {
                return null;
            }
        }
    }
}
