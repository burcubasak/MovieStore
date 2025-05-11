using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.User
{
  
    public record LoginUserQuery(LoginRequest LoginData) : IRequest<LoginResponse>;

    public record GetUserByIdQuery(Guid UserId) : IRequest<UserResponse?>; 
    public record GetAllUsersQuery() : IRequest<IEnumerable<UserResponse>>;
}
