using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Orders
{
    public class OrderCommandHandler :
        IRequestHandler<CreateOrderCommand, OrderResponse>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public OrderCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<OrderResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == command.UserId && u.IsActive, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException($"Active user with ID {command.UserId} not found.");
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == command.Request.MovieId && m.IsActive, cancellationToken);
            if (movie == null)
            {
                throw new KeyNotFoundException($"Active movie with ID {command.Request.MovieId} not found or is not available for purchase.");
            }

            var newOrder = new Order
            {
                UserId = user.Id,
                MovieId = movie.Id,
                PurchasedMovieTitle = movie.Title, 
                CustomerFullName = $"{user.FirstName} {user.LastName}", 
                Price = movie.Price, 
                OrderDate = DateTime.UtcNow,
                IsActive = true 
            };

            await _context.Orders.AddAsync(newOrder, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<OrderResponse>(newOrder);
        }
    }
}
