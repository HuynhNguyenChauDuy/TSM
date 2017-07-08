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

        public async Task<IEnumerable<LeaveVM>> GetLeavesAsync()
        {
            try
            {
                IEnumerable<LeaveVM> leaves = (from item in await (_context.Leaves
                                                 .Include(item => item.User)
                                                 .Include(item => item.LeaveType)
                                                 .OrderBy(item => item.FromDate)
                                                 .ThenBy(item => item.ToDate)).ToListAsync()
                                               select new LeaveVM()
                                               {
                                                   LeaveID = item.ID,
                                                   UserName = item.User.UserName,
                                                   FromDate = item.FromDate.ToString("dd/MM/yyyy"),
                                                   ToDate = item.ToDate.ToString("dd/MM/yyyy"),
                                                   SubmitedDate = item.SubmittedDate.ToString("dd/MM/yyyy"),
                                                   ApprovedDate = item.ApprovedDate.ToString("dd/MM/yyyy"),
                                                   WorkShift = item.WorkShift,
                                                   LeaveType = item.LeaveType.LeaveName,
                                                   State = item.State
                                               });

                return leaves;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SubmitLeaveAsync(Leave data, string userid)
        {
            try
            {
                data.ApplicationUserID = userid;
                data.SubmittedDate = DateTime.Now;
                data.State = Leave.eState.OnQueue;
                data.ApproverID = null;
                data.ApprovedDate = DateTime.Now;

                await _context.Leaves.AddAsync(data);
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
                var currentleave = await _context.Leaves.FindAsync(request.LeaveID);
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
    }
}
