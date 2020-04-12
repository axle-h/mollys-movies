using System.Linq;
using AutoMapper;
using MolliesMovies.Transmission.Data;
using MolliesMovies.Transmission.Models;

namespace MolliesMovies.Transmission
{
    public class TransmissionProfile : Profile
    {
        public TransmissionProfile()
        {
            CreateMap<TransmissionContext, TransmissionContextDto>()
                .ForMember(x => x.Status,
                    o => o.MapFrom(x => x.Statuses
                        .OrderByDescending(s => s.DateCreated)
                        .Select(s => s.Status)
                        .FirstOrDefault()));
        }
    }
}