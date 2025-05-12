using MediatR;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Order
{
    public record GetOrdersForUserQuery(Guid UserId) : IRequest<IEnumerable<OrderResponse>>;

    public record GetOrderByIdForUserQuery(Guid OrderId, Guid UserId) : IRequest<OrderResponse?>;
}