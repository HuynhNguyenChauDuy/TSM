using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace TSM.Data.ModelViews
{
    public class LeaveFormVM
    {
        public string ID { get; set; }

        [Required(ErrorMessage = "Please pick up a date")]
        [Display(Name = "From Date")]
        [DataType(DataType.DateTime)]
        public DateTime FromDate { get; set; }
    
        [Required(ErrorMessage = "Please pick up a date")]
        [Display(Name = "To Date")]
        [DataType(DataType.DateTime)]
        public DateTime ToDate { get; set; }

        [Display(Name = "WorkShift")]
        public int WorkShift { get; set; }

        [Display(Name = "Leave Type")]
        public string LeaveTypeID { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "CC")]
        public string CCId { get; set; }

    }

    public class LeaveFormVMValidator : AbstractValidator<LeaveFormVM>
    {
        public LeaveFormVMValidator()
        {
            //RuleFor(reg => reg.FromDate).GreaterThanOrEqualTo(DateTime.Today);
            //RuleFor(reg => reg.ToDate).GreaterThanOrEqualTo(DateTime.Today);
        }
    }

}
