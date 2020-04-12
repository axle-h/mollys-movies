using System.Linq;
using Microsoft.EntityFrameworkCore;
using MolliesMovies.Common.Data;

namespace MolliesMovies.Transmission.Data
{
    public static class TransmissionContextExtensions
    {
        public static IQueryable<TransmissionContext> TransmissionContexts(this MolliesMoviesContext context) =>
            context.Set<TransmissionContext>().Include(x => x.Statuses);

        public static TransmissionStatusCode GetStatus(this TransmissionContext context) =>
            context.Statuses.OrderByDescending(x => x.DateCreated).FirstOrDefault()?.Status ?? TransmissionStatusCode.Started;
    }
}