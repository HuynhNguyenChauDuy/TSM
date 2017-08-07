
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Models;
using static TSM.Data.Models.NotiSignalR;

namespace TSM.Hubs
{

    public class myHub:Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpContextAccessor _httpcontext;


        public myHub(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            HttpContextAccessor httpcontext
            )
        {
            _userManager = userManager;
            _context = context;
            _httpcontext = httpcontext;
        }

        public async Task<bool> ServerListener(eType type)
        {
            try
            {
                var userClaim = _httpcontext.HttpContext.User;
                var user = await _userManager.GetUserAsync(userClaim);
                string senderID = user.Id;
                if(type == eType.Request)
                {

                }
                    
                //Clients.Others.notifyUpdates(UserName);


                return true;
            }
            catch
            {
                return false;
            }
           
           
        }
    }
}
