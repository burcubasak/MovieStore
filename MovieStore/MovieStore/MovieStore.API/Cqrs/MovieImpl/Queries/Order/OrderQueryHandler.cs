using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Order
{
    public class OrderQueryHandler :
         IRequestHandler<GetOrdersForUserQuery, IEnumerable<OrderResponse>>,
         IRequestHandler<GetOrderByIdForUserQuery, OrderResponse?>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderQueryHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<OrderResponse>> Handle(GetOrdersForUserQuery request, CancellationToken cancellationToken)
        {

            var orders = await _context.Orders
                .Where(o => o.UserId == request.UserId && o.IsActive) 
                .OrderByDescending(o => o.OrderDate) 
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<OrderResponse>>(orders);
        }

        public async Task<OrderResponse?> Handle(GetOrderByIdForUserQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId && o.IsActive, cancellationToken);

            return _mapper.Map<OrderResponse>(order);
        }
    }
}
