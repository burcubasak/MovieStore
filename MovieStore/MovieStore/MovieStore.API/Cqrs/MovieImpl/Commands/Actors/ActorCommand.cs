using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors
{
    public record CreateActorCommand(ActorRequest Actor) : IRequest<ActorResponse>;
    public record UpdateActorCommand(Guid Id, ActorRequest Actor) : IRequest<ActorResponse>;
    public record DeleteActorCommand(Guid Id) : IRequest<ActorResponse>;

}
