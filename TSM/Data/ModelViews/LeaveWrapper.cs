using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data.Models;

namespace TSM.Data.ModelViews
{
    public class LeaveHandleVM_Multiple
    {
        public IEnumerable<string> LeaveID { get; set; }
        public Leave.eState Result { get; set; }
        public DateTime? CurDate { get; set; }
    }

    public class LeaveWrapper
    {
        public LeaveVM LeaveVMForDisplay { get; }
        public LeaveHandleVM LeaveHandleVM { get; set; }
        public LeaveFormVM LeaveFormVM { get; set; }
        public IEnumerable<LeaveVM> LeaveVM { get; set; }
        public LeaveHandleVM_Multiple LeaveHandleVM_Multiple { get; set; }
    }
}
