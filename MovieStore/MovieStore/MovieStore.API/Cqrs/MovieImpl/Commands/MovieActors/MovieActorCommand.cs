using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.MovieActors
{
    public class MovieActorCommand
    {
        public record LinkActorToMovieCommand(Guid MovieId, Guid ActorId) : IRequest<MovieActorStatusResponse>; 

        public record UnlinkActorFromMovieCommand(Guid MovieId, Guid ActorId, bool HardDelete = false) : IRequest<Unit>; 
    }
}