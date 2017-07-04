using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Models;

namespace TSM.Data.Models
{
    public class Team
    {
        public string ID { get; set; }
        public string TeamName { get; set; }
        public string Description { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
    }
}
