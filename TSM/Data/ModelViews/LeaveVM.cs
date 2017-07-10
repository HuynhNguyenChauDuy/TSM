using TSM.Data.Models;
using TSM.Models;

namespace TSM.Data.ModelViews
{
    public class LeaveVM
    {
        public string LeaveID { get; set; }
        public string UserName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
		public string ApprovedDate { get; set; }
        public string SubmittedDate { get; set; }
        public Leave.eWorkShift WorkShift { get; set; }
        public string LeaveType { get; set; }
		public Leave.eState State { get; set; }
        public string Note { get; set; }
    }
}
