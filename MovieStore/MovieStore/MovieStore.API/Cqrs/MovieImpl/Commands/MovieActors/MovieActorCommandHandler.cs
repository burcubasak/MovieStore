using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.MoviesActors;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.MovieActors.MovieActorCommand;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.MovieActors
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

}
