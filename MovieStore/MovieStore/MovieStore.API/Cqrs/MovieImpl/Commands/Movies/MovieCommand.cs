using MediatR;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies
{
    public record CreateMovieCommand(MovieRequest Movie) : IRequest<MovieResponse>;
    public record UpdateMovieCommand(int Id, MovieRequest Movie) : IRequest<MovieResponse>;
    public record DeleteMovieCommand(int Id) : IRequest<MovieResponse>;
}
