using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.User
{
    public class UserQueryHandler :
         IRequestHandler<LoginUserQuery, LoginResponse>,
         IRequestHandler<GetUserByIdQuery, UserResponse?>,
         IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration; // JWT ayarları için

        public UserQueryHandler(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<LoginResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.FirstName == request.LoginData.UserNameOrEmail || u.Email == request.LoginData.UserNameOrEmail) && u.IsActive, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid username/email or password."); // Kullanıcı bulunamadı veya pasif
            }

            // Şifre doğrulaması (Gerçek uygulamada Hash'lenmiş şifre ile karşılaştırılmalı)
            // bool isPasswordValid = VerifyPassword(request.LoginData.Password, user.PasswordHash); // Bu metodu implemente etmeniz gerekir.
            // ŞİMDİLİK BASİT BİR ÖRNEK (BCrypt.Net kullanarak):
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.LoginData.Password, user.Password);

            if (!isPasswordValid)
            {
                throw new InvalidOperationException("Invalid username/email or password.");
            }

            // JWT Oluşturma
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.FirstName), // Subject (kullanıcı adı)
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Benzersiz token ID'si
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID'si
                    // İsteğe bağlı olarak roller eklenebilir: new Claim(ClaimTypes.Role, "Customer")
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(
                    _configuration["JwtSettings:ExpirationHours"] ?? "1")), // Token geçerlilik süresi (appsettings'ten)
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new LoginResponse
            {
                Token = tokenString,
                Expiration = token.ValidTo,
                UserId = user.Id,
                UserName = user.FirstName,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        // Örnek şifre doğrulama metodu (BCrypt.Net kullanarak)
        // private bool VerifyPassword(string enteredPassword, string hashedPassword)
        // {
        //     return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        // }


        public async Task<UserResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<IEnumerable<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .Where(u => u.IsActive) // Sadece aktif kullanıcıları listele (genellikle adminler için)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }
    }
}
