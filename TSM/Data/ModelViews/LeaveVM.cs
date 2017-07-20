using System;
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

        [Display(Name = "To Date")]
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

        [Display(Name = "Approver")]
        public string Approver { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Default Sick Leave")]
        public int DefaultSickLeave { get; set; }

        [Display(Name = "Default Annual Leave")]
        public int DefaultAnnualLeave { get; set; }

        [Display(Name = "Sick Leaves")]
        public int NSickLeave { get; set; }

        [Display(Name = "Annual Leaves")]
        public int NAnnualLeave { get; set; }

        [Display(Name = "Other Leaves")]
        public int NOtherLeave { get; set; }
    }
}
