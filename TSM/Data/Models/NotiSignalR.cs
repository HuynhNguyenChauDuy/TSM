using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.Models
{
    public class NotiSignalR
    {
        public enum eType
        {
            Request,
            Disconnect,
            Have_message
        };

        public string ReceiverID { get; set; }
        public string SenderID { get; set; }
        public eType Type{ get; set; }
        public string Content { get; set; }
    }
}
