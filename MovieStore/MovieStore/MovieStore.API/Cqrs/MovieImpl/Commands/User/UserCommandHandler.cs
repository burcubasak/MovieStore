using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.User;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities; 
using MovieStore.MovieStore.Schema;    
using System;
using System.Threading;
using System.Threading.Tasks;
namespace MovieStore.MovieStore.API.Cqrs.UserImpl.Commands 
{

    public class UserCommandHandler :
        IRequestHandler<CreateUserCommand, UserResponse>,
        IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<UserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // DEBUG LOG: Gelen kullanıcı adını ve e-postayı kontrol etmek için
            Console.WriteLine($"DEBUG: Attempting to register user. UserName: '{request.UserData.UserName}', Email: '{request.UserData.Email}'");

            // Kullanıcı adı zaten var mı kontrol et (DOĞRU KONTROL)
            if (await _context.Users.AnyAsync(u => u.FirstName == request.UserData.UserName && u.IsActive, cancellationToken))
            {
                Console.WriteLine($"DEBUG: UserName '{request.UserData.UserName}' found in database. Throwing conflict.");
                throw new InvalidOperationException($"Username '{request.UserData.UserName}' is already taken.");
            }

            // E-posta zaten var mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == request.UserData.Email && u.IsActive, cancellationToken))
            {
                Console.WriteLine($"DEBUG: Email '{request.UserData.Email}' found in database. Throwing conflict.");
                throw new InvalidOperationException($"Email '{request.UserData.Email}' is already registered.");
            }

            var newUser = _mapper.Map<User>(request.UserData); 

            if (string.IsNullOrWhiteSpace(request.UserData.Password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(request.UserData.Password));
            }
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.UserData.Password);

            newUser.IsActive = true; 
            newUser.CreatedAt = DateTime.UtcNow;

            await _context.Users.AddAsync(newUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"DEBUG: User '{newUser.FirstName}' created successfully with ID '{newUser.Id}'.");
            return _mapper.Map<UserResponse>(newUser);
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var userToDelete = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

            if (userToDelete == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            }

            if (!userToDelete.IsActive)
            {
                throw new InvalidOperationException($"User with ID {request.UserId} is already inactive.");
            }

            userToDelete.IsActive = false;
            _context.Users.Update(userToDelete);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
