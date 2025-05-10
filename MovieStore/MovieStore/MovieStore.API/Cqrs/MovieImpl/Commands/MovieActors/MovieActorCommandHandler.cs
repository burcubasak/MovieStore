using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.MovieActors.MovieActorCommand;

namespace MovieStore.MovieStore.API.Cqrs.MovieActorImpl.Handlers
{
    public class MovieActorCommandHandler :
        IRequestHandler<LinkActorToMovieCommand, MovieActorStatusResponse>,
        IRequestHandler<UnlinkActorFromMovieCommand, Unit>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MovieActorCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<MovieActorStatusResponse> Handle(LinkActorToMovieCommand request, CancellationToken cancellationToken)
        {
            var movieExists = await _context.Movies.AnyAsync(m => m.Id == request.MovieId && m.IsActive, cancellationToken);
            if (!movieExists)
            {
                throw new KeyNotFoundException($"Active movie with ID {request.MovieId} not found.");
            }

            var actorExists = await _context.Actors.AnyAsync(a => a.Id == request.ActorId && a.IsActive, cancellationToken);
            if (!actorExists)
            {
                throw new KeyNotFoundException($"Active actor with ID {request.ActorId} not found.");
            }

            var movieActor = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == request.MovieId && ma.ActorId == request.ActorId, cancellationToken);

            if (movieActor == null)
            {
                movieActor = new MovieActor
                {
                    MovieId = request.MovieId,
                    ActorId = request.ActorId,
                    IsActive = true 
                };
                await _context.MovieActors.AddAsync(movieActor, cancellationToken);
            }
            else
            {
                if (!movieActor.IsActive)
                {
                    movieActor.IsActive = true; 
                    _context.MovieActors.Update(movieActor);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new MovieActorStatusResponse
            {
                MovieId = movieActor.MovieId,
                ActorId = movieActor.ActorId,
                IsActiveInRole = movieActor.IsActive
            };
        }

        public async Task<Unit> Handle(UnlinkActorFromMovieCommand request, CancellationToken cancellationToken)
        {
            var movieActor = await _context.MovieActors
                .FirstOrDefaultAsync(ma => ma.MovieId == request.MovieId && ma.ActorId == request.ActorId, cancellationToken);

            if (movieActor == null)
            {
                throw new KeyNotFoundException($"Link between movie ID {request.MovieId} and actor ID {request.ActorId} not found.");
            }

            if (request.HardDelete)
            {
                _context.MovieActors.Remove(movieActor);
            }
            else
            {
                if (movieActor.IsActive) 
                {
                    movieActor.IsActive = false;
                    _context.MovieActors.Update(movieActor);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }

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
