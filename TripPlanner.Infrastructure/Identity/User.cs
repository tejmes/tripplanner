using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TripPlanner.Domain.Entities.Interfaces;

namespace TripPlanner.Infrastructure.Identity
{
    public class User : IdentityUser<int>, IUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
