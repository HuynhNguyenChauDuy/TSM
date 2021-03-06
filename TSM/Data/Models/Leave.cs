﻿using System;
using System.Collections.Generic;
using TSM.Models;

namespace TSM.Data.Models
{
    public class Leave
    {
        public enum eWorkShift
        {
            AllDay,
            Morning,
            Afternoon
        };

        public enum eState
        {
            Approved,
            Rejected,
            OnQueue
        };

        public string ID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        public eWorkShift WorkShift { get; set; }
        public DateTime ApprovedDate { get; set; }
        public eState State { get; set; }
        public string Note { get; set; }
        public string ApproverID { get; set; }
        public string CCId { get; set; }
        public string ApplicationUserID { get; set; }
        public ApplicationUser User { get; set; }

        public string LeaveTypeID { get; set; }
        public LeaveType LeaveType { get; set; }
    }
}
