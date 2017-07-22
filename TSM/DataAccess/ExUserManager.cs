using AutoMapper;
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
        private readonly IMapper _mapper;

        public ExUserManager
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper
            )
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ProfileVM> GetUserDetailById(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(item => item.Team)
                    .Where(item => item.Id.CompareTo(userId) == 0).FirstOrDefaultAsync();

                var nSickleave = await CountLeaveByUserId(userId, "Sick Leave");
                var nAnnualLeave = await CountLeaveByUserId(userId, "AnnualLeave");
                var nOtherLeave = await CountLeaveByUserId(userId, "Other");

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
                    NSickLeave = nSickleave
                };

                
                    return profileVM;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private async Task<int> CountLeaveByUserId(string userId, string LeaveType)
        {
            try
            {
                return await _context.Leaves
                    .Include(item => item.LeaveType)
                    .Where(item => item.ApplicationUserID.CompareTo(userId) == 0
                                        && item.LeaveType.LeaveName.CompareTo(LeaveType) == 0
                                        && item.State == Leave.eState.Approved)
                    .CountAsync();
            }
            catch
            {
                return 0;
            }
        } 
        
        public async Task<bool> EditProfile(ProfileVM submit, string userId)
        {
            try
            {
               ApplicationUser curProfile = await _context.Users
                    .Where(item => item.Id.CompareTo(userId) == 0).FirstOrDefaultAsync();
                if(curProfile == null)
                {
                    return false;
                }
                curProfile.Email = submit.Email;
                curProfile.PhoneNumber = submit.PhoneNumber;
                _context.Entry(curProfile).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

       
    }
}
