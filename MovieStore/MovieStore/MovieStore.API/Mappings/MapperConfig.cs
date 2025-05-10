using AutoMapper;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;

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
        }
    }
}
