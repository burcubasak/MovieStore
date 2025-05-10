using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies
{
    public class MovieActorQueryHandler :
      IRequestHandler<GetActiveActorsForMovieQuery, IEnumerable<ActorResponse>>,
      IRequestHandler<GetActiveMoviesForActorQuery, IEnumerable<MovieResponse>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MovieActorQueryHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ActorResponse>> Handle(GetActiveActorsForMovieQuery request, CancellationToken cancellationToken)
        {
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId && m.IsActive, cancellationToken);
            if (!movieExists)
            {
                return Enumerable.Empty<ActorResponse>();
            }

            var actors = await _context.MovieActors
                .Where(ma => ma.MovieId == request.MovieId && ma.IsActive && ma.Actor.IsActive) 
                .Select(ma => ma.Actor)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<ActorResponse>>(actors);
        }

        public async Task<IEnumerable<MovieResponse>> Handle(GetActiveMoviesForActorQuery request, CancellationToken cancellationToken)
        {
            var actorExists = await _context.Actors.AnyAsync(a => a.Id == request.ActorId && a.IsActive, cancellationToken);
            if (!actorExists)
            {
                return Enumerable.Empty<MovieResponse>();
            }

            var movies = await _context.MovieActors
                .Where(ma => ma.ActorId == request.ActorId && ma.IsActive && ma.Movie.IsActive) 
                .Select(ma => ma.Movie)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<MovieResponse>>(movies);
        }
    }
}
