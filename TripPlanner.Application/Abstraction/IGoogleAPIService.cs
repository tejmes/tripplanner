using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.Abstraction
{
    public interface IGoogleAPIService
    {
        Task<string?> GetPhoto(string googlePlaceId, int maxWidth = 400);
    }
}
