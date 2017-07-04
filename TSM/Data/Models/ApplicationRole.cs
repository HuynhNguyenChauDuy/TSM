using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSM.Data.Models
{
    enum UserRoleType
    {
        Staff,
        Team_leader,
        Project_Manager
    };

    public class ApplicationRole : IdentityRole 
    {

    }
}
