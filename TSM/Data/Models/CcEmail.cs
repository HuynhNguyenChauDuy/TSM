using TSM.Models;

namespace TSM.Data.Models
{
    public class CcEmail
    {
        public string LeaveID { get; set; }
        public string ApplicationUserID { get; set; }

        public ApplicationUser User { get; set; }
    }
}
