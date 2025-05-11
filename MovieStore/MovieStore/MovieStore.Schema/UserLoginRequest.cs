using System;
using System.Collections.Generic;
using MovieStore.MovieStore.API.Entities;
using static MovieStore.MovieStore.API.Entities.Enum;
namespace MovieStore.MovieStore.Schema
{
   
    public class UserCreateRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string UserName { get; set; } = null!; 
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

        public List<MovieGenre> FavoriteGenres { get; set; } = new List<MovieGenre>();
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<MovieGenre> FavoriteGenres { get; set; } = new List<MovieGenre>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LoginRequest
    {
        public string UserNameOrEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
