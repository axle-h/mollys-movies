using AutoMapper;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Common
{
    public class CommonProfile : Profile
    {
        public CommonProfile()
        {
            CreateMap(typeof(PaginatedData<>), typeof(Paginated<>));
        }
    }
}