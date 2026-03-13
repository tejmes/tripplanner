using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface ITripDetailService
    {
        Task<TripDetailViewModel> GetTripDetail(int tripId, int userId);
    }
}
