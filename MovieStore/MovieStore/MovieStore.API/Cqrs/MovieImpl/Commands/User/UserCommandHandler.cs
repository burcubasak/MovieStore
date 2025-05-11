namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.User
{
    using AutoMapper;
    using global::MovieStore.MovieStore.API.DbContexts;
    using global::MovieStore.MovieStore.Schema;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
  

    namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.User
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
                // Kullanıcı adı veya e-posta zaten var mı kontrol et
                if (await _context.Users.AnyAsync(u => u.FirstName == request.UserData.FirstName, cancellationToken))
                {
                    throw new InvalidOperationException($"Username '{request.UserData.UserName}' is already taken.");
                }
                if (await _context.Users.AnyAsync(u => u.Email == request.UserData.Email, cancellationToken))
                {
                    throw new InvalidOperationException($"Email '{request.UserData.Email}' is already registered.");
                }

                var newUser = _mapper.Map<Entities.User>(request.UserData);

                // Şifreyi hash'le (Gerçek bir uygulamada güçlü bir hash algoritması kullanılmalı)
                // Örnek olarak BCrypt.Net veya ASP.NET Core Identity'nin PasswordHasher'ı kullanılabilir.
                // Basitlik adına burada düz metin gibi görünecek ama gerçekte hash'lenmeli.
                // newUser.PasswordHash = HashPassword(request.UserData.Password); // Bu metodu implemente etmeniz gerekir.
                // ŞİMDİLİK BASİT BİR ÖRNEK (GÜVENLİ DEĞİL, GERÇEK UYGULAMADA KULLANMAYIN!):
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.UserData.Password);


                newUser.IsActive = true; // Yeni kullanıcı varsayılan olarak aktif
                newUser.CreatedAt = DateTime.UtcNow;

                await _context.Users.AddAsync(newUser, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return _mapper.Map<UserResponse>(newUser);
            }

            // Örnek şifre hash'leme metodu (BCrypt.Net kullanarak)
            // private string HashPassword(string password)
            // {
            //     return BCrypt.Net.BCrypt.HashPassword(password);
            // }

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

                // Soft delete: Kullanıcıyı pasif hale getir
                userToDelete.IsActive = false;
                _context.Users.Update(userToDelete);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }

}
