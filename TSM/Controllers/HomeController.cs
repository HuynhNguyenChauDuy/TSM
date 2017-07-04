using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TSM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TSM.Data;

namespace TSM.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _sysRole;

        private Task<ApplicationUser> GetCurrentUser()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        public HomeController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> sysRole)
        {
            _userManager = userManager;
            _sysRole = sysRole;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user != null)
            {
                //var RoleName = await _userManager.GetClaimsAsync(user);
                var RoleName = await _userManager.GetRolesAsync(user);
                return View(RoleName);
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
