using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Orders
{
    public record CreateOrderCommand(Guid UserId, OrderCreateRequest Request) : IRequest<OrderResponse>;

}
