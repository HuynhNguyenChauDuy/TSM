using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Models;

namespace TSM.Data.ModelViews
{
    public class ProfileVM
    {
        public IFormFile AvatarImage { get; set; }
        public string Email { get; set; }
		public string UserName { get; set; }
		public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string TeamName { get; set; }
        
        public int DefaultSickLeave { get; set; }
        public int DefaultAnnualLeave { get; set; }

        public int NSickLeave { get; set; }
        public int NAnnualLeave { get; set; }
        public int NOther { get; set; }


        public int NApprovedLeaves { get; set; }
        public int NRejectedLeaves { get; set; }
        public int NWaitingLeaves { get; set; }

        public int NApprovedDates { get; set; }
        public int NRejectedDates { get; set; }
        public int NAwaitingDates { get; set; }

        public static implicit operator ProfileVM(ApplicationUser v)
        {
            throw new NotImplementedException();
        }
    }
}
