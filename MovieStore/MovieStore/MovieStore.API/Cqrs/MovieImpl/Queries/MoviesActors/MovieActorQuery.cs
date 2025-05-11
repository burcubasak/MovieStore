using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.MoviesActors
{
    public record GetActiveActorsForMovieQuery(Guid MovieId) : IRequest<IEnumerable<ActorResponse>>;
    public record GetActiveMoviesForActorQuery(Guid ActorId) : IRequest<IEnumerable<MovieResponse>>;
}
