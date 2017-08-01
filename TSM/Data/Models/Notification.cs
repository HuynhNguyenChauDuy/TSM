using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Models;

namespace TSM.Data.Models
{
    public enum NotiType
    {
        Request
    }

    public class Notification
    {
        public string ID { get; set; }
        public string UserId { get; set; }
        public string ReceiverId { get; set;}
        public NotiType NotiType { get; set; }
        public string Content { get; set; }

        public ApplicationUser User { get; set; }
    }
}
