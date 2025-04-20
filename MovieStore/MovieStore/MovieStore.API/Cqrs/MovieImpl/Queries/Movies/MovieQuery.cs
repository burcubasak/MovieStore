using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies
{
    public record MovieIdByQuery(Guid Id):IRequest<MovieResponse>;
    public record GetAllMovieQuery:IRequest<List<MovieResponse>>;

  
}
