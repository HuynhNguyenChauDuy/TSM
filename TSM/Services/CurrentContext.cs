using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.Models.ManageViewModels;

namespace TSM.Services
{
    public class CurrentContext
    {
        private readonly ApplicationDbContext _context;

        public CurrentContext(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<LeaveTypeVM> GetLeaveType()
        {
            return from type in _context.LeaveTypes.ToList()
                   select new LeaveTypeVM()
                   {
                       ID = type.ID,
                       Name = type.LeaveName
                   };
        }
    }

}
