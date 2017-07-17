using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.ModelViews;
using TSM.Models;
using Microsoft.EntityFrameworkCore;

namespace TSM.DataAccess
{
    public class ExUserManager
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ExUserManager
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper
            )
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ProfileVM> GetUserDetailById(string userId)
        {
            try
            {
                var user = _context.Users
                    .Include(item => item.Team)
                    .Include(item => item.Roles)
                    
                    .Where(item => item.Id.CompareTo(userId) == 0).FirstOrDefault();

                var userRoles = await _userManager.GetRolesAsync(user);

                var profileVM = new ProfileVM()
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Team = user.Team.TeamName,
                    Position = userRoles[0] 

                };
                return profileVM;
            }
            catch (Exception ex)
            {
                var k = ex.Message;
                return null;
            }
        }

    }
}
