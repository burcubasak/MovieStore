using AutoMapper;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;
using static MovieStore.MovieStore.API.Entities.Enum;

namespace MovieStore.MovieStore.API.Mappings
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<MovieRequest, Movie>();
            CreateMap<Movie, MovieResponse>();

            CreateMap<ActorRequest, Actor>();
            CreateMap<Actor, ActorResponse>();

            CreateMap<DirectorRequest, Director>(); 
            CreateMap<Director, DirectorResponse>();

            CreateMap<UserCreateRequest, User>()
                 .ForMember(dest => dest.Password, opt => opt.Ignore()) 
                 .ForMember(dest => dest.FavoriteGenres, opt => opt.MapFrom(src => src.FavoriteGenres.Select(genre => genre.ToString()).ToList())) 
                 .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                 .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) 
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)); 

           
            CreateMap<User, UserResponse>()

                  .ForMember(dest => dest.FavoriteGenres, opt => opt.MapFrom(src => src.FavoriteGenres.Select(genre => genre.ToString()).ToList())); 

        }
    }
}
