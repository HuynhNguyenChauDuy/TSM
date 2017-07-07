using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.ModelViews
{
    public class LeaveHandleVM
    {
        public string LeaveID { get; set; }
        public string ApproverID{ get; set; }
        public string ApproveDate { get; set; }
        public Models.Leave.eState Result { get; set; }
    }
}
