using System.ComponentModel.DataAnnotations;
using TSM.Data.Models;
namespace TSM.Data.ModelViews
{
    public class LeaveVM
    {
        public string LeaveID { get; set; }

        [Display(Name = "Employee")]
        public string UserName { get; set; }

        [Display(Name = "From Date")]
        public string FromDate { get; set; }

        [Display(Name = "To date")]
        public string ToDate { get; set; }

        [Display(Name = "Approved/Rejected Date")]
        public string ApprovedDate { get; set; }

        [Display(Name = "Submitted Date")]
        public string SubmittedDate { get; set; }

        [Display(Name = "Work Shift")]
        public Leave.eWorkShift WorkShift { get; set; }

        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; }

        [Display(Name = "State")]
        public Leave.eState State { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }
    }
}
