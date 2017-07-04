using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
namespace TSM.Data.Models.ManageViewModels
{
    public class LeaveFormVM
    {
        [Required(ErrorMessage = "Please pick up a date")]
        [Display(Name = "From Date")]
        [DataType(DataType.DateTime)]
        public DateTime FromDate { get; set; }
    
        [Required(ErrorMessage = "Please pick up a date")]
        [Display(Name = "To Date")]
        [DataType(DataType.DateTime)]
        public DateTime ToDate { get; set; }

        //[DataType(DataType.DateTime)]
        //public DateTime SubmittedDate { get; set; }

        [Display(Name = "WorkShift")]
        public int WorkShift { get; set; }

        [Display(Name = "Leave Type")]
        public string LeaveTypeID { get; set; }

        [Display(Name = "Note")]
        [MaxLengthAttribute]
        public string Note { get; set; }

    }

    public class LeaveFormVMValidator : AbstractValidator<LeaveFormVM>
    {
        public LeaveFormVMValidator()
        {
            RuleFor(reg => reg.FromDate)
                .GreaterThanOrEqualTo(DateTime.Today);

            // doesn't work
           // RuleFor(reg => reg.FromDate)
           //      .GreaterThanOrEqualTo(reg2 => reg2.FromDate);
        }
    }

}
