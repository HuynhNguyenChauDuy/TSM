using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models;
using TSM.Data.Models.ManageViewModels;
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

        public async Task<IEnumerable<LeaveTypeVM>> GetLeaveType()
        {
            try
            {
                var LeavetypeVM = from type in await _context.LeaveTypes.ToListAsync()
                                  select new LeaveTypeVM()
                                  {
                                      ID = type.ID,
                                      LeaveName = type.LeaveName
                                  };

                return LeavetypeVM;
               
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<LeaveVM>> GetLeaves()
        {
            try
            {
                var Leaves = (from item in await (_context.Leaves
                                                .Include(item => item.User)
                                                .Include(item => item.LeaveType)
                                                .OrderByDescending(item => item.State)
                                                .ToListAsync())
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

                return Leaves;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SubmitLeave(Leave Data, string UserID)
        {
            try
            {
                Data.ApplicationUserID = UserID;
                Data.SubmittedDate = DateTime.Today;
                Data.State = Leave.eState.OnQueue;
                Data.ApproverID = null;
                Data.ApprovedDate = DateTime.Today;

                await _context.Leaves.AddAsync(Data);
                _context.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
