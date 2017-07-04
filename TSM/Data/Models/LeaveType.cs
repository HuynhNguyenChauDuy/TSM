using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.Models
{
    public class LeaveType
    {
        public string ID { get; set; }
        public string LeaveName { get; set;}
        public bool Paid { get; set; }

        public ICollection<Leave>  Leaves { get; set; }
    }
}
