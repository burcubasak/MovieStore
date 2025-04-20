using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors
{
    public record GetAllActorQuery:IRequest<List<ActorResponse>>;
    public record ActorIdByQuery(Guid Id) : IRequest<ActorResponse>;

}
