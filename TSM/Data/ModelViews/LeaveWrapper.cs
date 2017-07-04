using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.Models.ManageViewModels
{
    public class LeaveWrapper
    {
        public LeaveFormVM LeaveFormVM { get; set; }
        public IEnumerable<LeaveVM> LeaveVM { get; set; }
    }
}
