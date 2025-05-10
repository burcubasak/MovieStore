using MediatR;
using MovieStore.MovieStore.Schema; 


namespace MovieStore.MovieStore.API.Cqrs.DirectorImpl.Commands
{
    public record CreateDirectorCommand(DirectorRequest Director) : IRequest<DirectorResponse>;
    public record UpdateDirectorCommand(Guid Id, DirectorRequest Director) : IRequest<DirectorResponse>;
    public record DeleteDirectorCommand(Guid Id) : IRequest<DirectorResponse>;
}
