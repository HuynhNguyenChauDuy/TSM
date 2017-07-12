using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TSM.Data;
using TSM.Data.ModelViews;
using TSM.Models;

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
    }
}
