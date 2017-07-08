using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.ModelViews
{
    public class LeaveWrapper
    {
        public LeaveHandleVM LeaveHandleVM { get; set; }
        public LeaveFormVM LeaveFormVM { get; set; }
        public IEnumerable<LeaveVM> LeaveVM { get; set; }
    }
}
