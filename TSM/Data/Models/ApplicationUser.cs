using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TSM.Data.Models;

namespace TSM.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
       
        public int DefaultSickLeave { get; set; }
        public int DefaultAnnualLeave { get; set; }

        public string TeamID { get; set; }
        public Team Team { get; set; }

        public ICollection<Leave> Leaves { get; set; }
        public ICollection<Notification> Notifications { get; set; }

    }
}
