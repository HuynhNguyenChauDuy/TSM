using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;
using TSM.Data.ModelViews;
using TSM.Models;

namespace TSM.DataAccess
{
    public class LeaveManager
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public LeaveManager(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LeaveTypeVM>> GetLeaveTypeAsync()
        {
            try
            {
                IEnumerable<LeaveTypeVM> leaveTypeVm = from type in await _context.LeaveTypes.ToListAsync()
                                                       select new LeaveTypeVM()
                                                       {
                                                           ID = type.ID,
                                                           LeaveName = type.LeaveName
                                                       };

                return leaveTypeVm;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<LeaveVM>> GetAllLeavesAsync(DateTime? date = null)
        {
            try
            {
                IEnumerable<Leave> leaves = null;

                if(date != null)
                {
                    leaves = await _context.Leaves
                                                 .Include(item => item.User)
                                                 .Include(item => item.LeaveType)
                                                 .Where(item => (item.FromDate <= date && date >= item.ToDate))
                                                 .OrderBy(item => item.FromDate)
                                                 .ThenBy(item => item.ToDate)
                                                 .ToListAsync();
                }
                else
                {
                    leaves = await _context.Leaves
                                                 .Include(item => item.User)
                                                 .Include(item => item.LeaveType)
                                                 .Where(item => (item.FromDate <= DateTime.Today && DateTime.Today <= item.ToDate))
                                                 .OrderBy(item => item.FromDate)
                                                 .ThenBy(item => item.ToDate)
                                                 .ToListAsync();
                }

                var leavevms = from item in leaves
                               select new LeaveVM()
                               {
                                   LeaveID = item.ID,
                                   UserName = item.User.UserName,
                                   FromDate = item.FromDate.ToString("dd/MM/yyyy"),
                                   ToDate = item.ToDate.ToString("dd/MM/yyyy"),
                                   SubmittedDate = null,
                                   ApprovedDate = null,
                                   WorkShift = item.WorkShift,
                                   LeaveType = item.LeaveType.LeaveName,
                                   State = item.State,
                                   Note = null
                               };

                return leavevms;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<LeaveVM>> GetLeavesByTeamIdAsync(string teamID)
        {
            try
            {
                IEnumerable<LeaveVM> leaves = (from item in await (_context.Leaves
                                                 .Include(item => item.User).Where(item => item.User.TeamID.CompareTo(teamID) == 0)
                                                 .Include(item => item.LeaveType)
                                                 .OrderBy(item => item.FromDate)
                                                 .ThenBy(item => item.ToDate)).ToListAsync()
                                               select new LeaveVM()
                                               {
                                                   LeaveID = item.ID,
                                                   UserName = item.User.UserName,
                                                   FromDate = item.FromDate.ToString("dd/MM/yyyy"),
                                                   ToDate = item.ToDate.ToString("dd/MM/yyyy"),
                                                   SubmittedDate = null,
                                                   ApprovedDate = null,
                                                   WorkShift = item.WorkShift,
                                                   LeaveType = item.LeaveType.LeaveName,
                                                   State = item.State,
                                                   Note = null
                                               });

                return leaves;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<LeaveVM>> GetLeavebyUserIdsAsync(string userId)
        {
            try
            {
                IEnumerable<LeaveVM> leaves = (from item in await (_context.Leaves
                                                 .Include(item => item.User).Where(item => item.ApplicationUserID.CompareTo(userId) == 0)
                                                 .Include(item => item.LeaveType)
                                                 .OrderBy(item => item.FromDate)
                                                 .ThenBy(item => item.ToDate)).ToListAsync()
                                               select new LeaveVM()
                                               {
                                                   LeaveID = item.ID,
                                                   UserName = item.User.UserName,
                                                   FromDate = item.FromDate.ToString("dd/MM/yyyy"),
                                                   ToDate = item.ToDate.ToString("dd/MM/yyyy"),
                                                   SubmittedDate = null,
                                                   ApprovedDate = null,
                                                   WorkShift = item.WorkShift,
                                                   LeaveType = item.LeaveType.LeaveName,
                                                   State = item.State,
                                                   Note = null
                                               });

                return leaves;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<int> CountLeavesByUserId(string userId, string LeaveType)
        {
            try
            {
                int count = 0;

                foreach(var item in (_context.Leaves
                                           .Include(item => item.LeaveType)
                                           .Where(item => item.ApplicationUserID.CompareTo(userId) == 0
                                           && item.LeaveType.LeaveName.CompareTo(LeaveType) == 0
                                           && item.State == Leave.eState.Approved)))
                {
                    count += (item.ToDate - item.FromDate).Days == 0 ? 1 : (item.ToDate - item.FromDate).Days + 1;
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<LeaveVM> GetLeaveDetailForManager(string leaveId)
        {
            try
            {
                // get current leave
                Leave leave = await _context.Leaves
                    .Include(item => item.User)
                    .Include(item => item.LeaveType)
                    .Where(item => item.ID.CompareTo(leaveId) == 0)
                    .FirstOrDefaultAsync();

                // approved/reject date
                var approvedDate = leave.State == Leave.eState.OnQueue ?
                                            "--/--/----" : leave.ApprovedDate.ToString("dd/MM/yyy");

                // get approver's name
                var approverName = "---------";
                if(leave.ApproverID != null)
                {
                    approverName = (await _context.Users
                   .Where(item => item.Id.CompareTo(leave.ApproverID) == 0)
                   .FirstOrDefaultAsync()).UserName;
                }

                // get leave owner
                var user = _context.Users.Find(leave.ApplicationUserID);

                // get leave count
                var nSickleave = await CountLeavesByUserId(user.Id, "Sick Leave");
                var nAnnualLeave = await CountLeavesByUserId(user.Id, "Annual Leave");
                var nOtherLeave = await CountLeavesByUserId(user.Id, "Other");

                LeaveVM leaveVM = new LeaveVM()
                {
                    LeaveID = leave.ID,
                    UserName = leave.User.UserName,
                    FromDate = leave.FromDate.ToString("dd/MM/yyyy"),
                    ToDate = leave.ToDate.ToString("dd/MM/yyyy"),
                    SubmittedDate = leave.SubmittedDate.ToString("dd/MM/yyyy"),
                    ApprovedDate = approvedDate,
                    WorkShift = leave.WorkShift,
                    LeaveType = leave.LeaveType.LeaveName,
                    State = leave.State,
                    Note = System.Net.WebUtility.HtmlDecode(leave.Note),
                    Approver = approverName,
                    DefaultAnnualLeave = user.DefaultAnnualLeave,
                    DefaultSickLeave = user.DefaultSickLeave,
                    NAnnualLeave = nAnnualLeave,
                    NOtherLeave = nOtherLeave,
                    NSickLeave = nSickleave
                };

                return leaveVM;
            }
            catch
            {
                return null;
            }
        }

        public async Task<LeaveVM> GetLeaveDetail(string leaveId)
        {
            try
            {
                // get current leave
                Leave leave = await _context.Leaves
                    .Include(item => item.User)
                    .Include(item => item.LeaveType)
                    .Where(item => item.ID.CompareTo(leaveId) == 0)
                   .FirstOrDefaultAsync();

                // approved/reject date
                var approvedDate = leave.State == Leave.eState.OnQueue ?
                                            "--/--/----" : leave.ApprovedDate.ToString("dd/MM/yyy");

                // get approver's name
                var approverName = "---------";
                if(leave.ApproverID != null)
                {
                    approverName = (await _context.Users
                   .Where(item => item.Id.CompareTo(leave.ApproverID) == 0)
                   .FirstOrDefaultAsync()).UserName;
                }

                LeaveVM leaveVM = new LeaveVM()
                {
                    LeaveID = leave.ID,
                    UserName = leave.User.UserName,
                    FromDate = leave.FromDate.ToString("dd/MM/yyyy"),
                    ToDate = leave.ToDate.ToString("dd/MM/yyyy"),
                    SubmittedDate = leave.SubmittedDate.ToString("dd/MM/yyyy"),
                    ApprovedDate = approvedDate,
                    WorkShift = leave.WorkShift,
                    LeaveType = leave.LeaveType.LeaveName,
                    State = leave.State,
                    Note = System.Net.WebUtility.HtmlDecode(leave.Note),
                    Approver = approverName
                };

                return leaveVM;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> DateInputIsValidAsync(DateTime fromDate, DateTime toDate, string userId)
        {
            foreach (var item in _context.Leaves
                .Where(item => item.ApplicationUserID
                .CompareTo(userId) == 0))
            {
                if ((fromDate >= item.FromDate && fromDate <= item.ToDate) || (toDate >= item.FromDate && toDate <= item.ToDate))
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Leave> SubmitLeaveAsync(Leave leave, ApplicationUser user)
        {
            try
            {
                if(!(await DateInputIsValidAsync(leave.FromDate, leave.ToDate, user.Id)))
                {
                    return null;
                }

                IList<string> userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains("Project Manager"))
                {
                    leave.State = Leave.eState.Approved;
                    leave.ApproverID = user.Id;
                }
                else
                {
                    leave.State = Leave.eState.OnQueue;
                    leave.ApproverID = null;
                }

                leave.ApplicationUserID = user.Id;
                leave.SubmittedDate = DateTime.Now;
                leave.ApprovedDate = DateTime.Now;

                await _context.Leaves.AddAsync(leave);
                await _context.SaveChangesAsync();

                return leave;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> EditLeaveAsync(Leave leave, string userId)
        {
            try
            {
                Leave curLeave = await _context.Leaves
                    .Where(item => item.ID.CompareTo(leave.ID) == 0 && item.ApplicationUserID.CompareTo(userId) == 0)
                    .FirstOrDefaultAsync();

                if(curLeave == null || curLeave.State != Leave.eState.OnQueue)
                {
                    return false;
                }

                // temporarily put this condition code snipps above, seperate it if possible
                foreach(var item in _context.Leaves
                        .Where(item => item.ApplicationUserID
                        .CompareTo(userId) == 0 && item.ID.CompareTo(leave.ID) != 0))
                {
                    if ((leave.FromDate >= item.FromDate && leave.FromDate <= item.ToDate) || (leave.ToDate >= item.FromDate && leave.ToDate <= item.ToDate))
                    {
                        return false;
                    }
                }

                curLeave.FromDate = leave.FromDate;
                curLeave.ToDate = leave.ToDate;
                curLeave.WorkShift = leave.WorkShift;
                curLeave.LeaveTypeID = leave.LeaveTypeID;
                curLeave.Note = leave.Note;

                _context.Entry(curLeave).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

		public async Task<bool> DeleteLeaveAsync(string id, string userid)
		{
			try
			{
                Leave leave = await _context.Leaves
                    .Where(s => s.ID.CompareTo(id) == 0 && s.ApplicationUserID.CompareTo(userid) == 0)
                    .FirstOrDefaultAsync();
				if (leave == null)
				{
					return false;
				}
				
				_context.Entry(leave).State = EntityState.Deleted;
				_context.Leaves.Remove(leave);

				await _context.SaveChangesAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<bool> HandleSingleRequestAysnc(LeaveHandleVM request, string userId)
        {
            try
            {
                Leave currentleave = await _context.Leaves.FindAsync(request.LeaveID);
                if(currentleave.State != Leave.eState.OnQueue)
                {
                    return false;
                }

                currentleave.ApproverID = userId;
                currentleave.State = request.Result;
                currentleave.ApprovedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> HandleMultipleRequestsAsync(LeaveHandleVM_Multiple requests, string userId)
        {
            try
            {
                var leaveIdList = new List<string>();
                foreach(var item in requests.LeaveID)
                {
                    var currentleave = await _context.Leaves.FindAsync(item);
                    if (currentleave.State == Leave.eState.OnQueue)
                    {
                        leaveIdList.Add(item);

                        currentleave.ApproverID = userId;
                        currentleave.State = requests.Result;
                        currentleave.ApprovedDate = DateTime.Now;
                        _context.Entry(currentleave).State = EntityState.Modified;
                    }
                }
                await _context.SaveChangesAsync();

                return leaveIdList;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
