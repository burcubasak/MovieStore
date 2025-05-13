using System; // Ensure this is at the top of the file
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MovieStore.MovieStore.API.Entities; // User, Actor, Director, Movie, Order etc.
using MovieStore.MovieStore.Schema;   // UserCreateRequest, UserResponse, ActorRequest etc.
using MovieGenreEnum = MovieStore.MovieStore.API.Entities.Enum.MovieGenre; // Alias to avoid ambiguity
using SystemEnum = System.Enum; // Alias System.Enum to avoid ambiguity
using static MovieStore.MovieStore.API.Entities.Enum;

namespace MovieStore.MovieStore.API.Mappings // Bu namespace projenizdekiyle eşleşmeli
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            // === Actor Mappings ===
            CreateMap<ActorRequest, Actor>();
            CreateMap<Actor, ActorResponse>();

            // === Director Mappings ===
            CreateMap<DirectorRequest, Director>();
            CreateMap<Director, DirectorResponse>();

            // === Movie Mappings ===
            // CreateMap<MovieRequest, Movie>(); 
            // CreateMap<Movie, MovieResponse>(); 

            // === User Mappings ===
            // UserCreateRequest DTO to User entity mapping
            CreateMap<UserCreateRequest, User>()
            .ForMember(dest => dest.FavoriteGenres, opt => opt.MapFrom(src =>
                src.FavoriteGenres == null || !src.FavoriteGenres.Any()
                ? string.Empty
                : string.Join(",", src.FavoriteGenres.Select(genreEnum => genreEnum.ToString()))
            ));

            CreateMap<User, UserResponse>()
      .ForMember(dest => dest.FavoriteGenres, opt => opt.MapFrom(src =>
          string.IsNullOrEmpty(src.FavoriteGenres)
          ? new List<MovieGenreEnum>()
          : src.FavoriteGenres.Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(genreNameString => SystemEnum.Parse<MovieGenreEnum>(genreNameString))
              .ToList()
      ));





            // === Order Mappings ===
            CreateMap<OrderCreateRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.PurchasedMovieTitle, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerFullName, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.PurchasedMovieTitle))
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.CustomerFullName));
        }
    }
}
