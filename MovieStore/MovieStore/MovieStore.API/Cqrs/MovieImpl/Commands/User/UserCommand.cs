using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.User
{
  
    public record CreateUserCommand(UserCreateRequest UserData) : IRequest<UserResponse>;

    public record DeleteUserCommand(Guid UserId) : IRequest<Unit>;
}
