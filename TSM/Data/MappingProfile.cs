﻿using AutoMapper;
using System.Collections.Generic;
using TSM.Data.Models;
using TSM.Data.ModelViews;
using TSM.Models;

namespace TSM.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Leave, LeaveVM>();
            CreateMap<LeaveFormVM, Leave>();
            CreateMap<List<LeaveType>, List<LeaveTypeVM>>();
            CreateMap<List<ApplicationUser>, List<CCVM>>();
        }
    }
}
