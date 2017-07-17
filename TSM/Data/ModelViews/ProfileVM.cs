using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.ModelViews
{
    public class ProfileVM
    {
		public string Email { get; set; }
		public string UserName { get; set; }
		public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
    }
}
