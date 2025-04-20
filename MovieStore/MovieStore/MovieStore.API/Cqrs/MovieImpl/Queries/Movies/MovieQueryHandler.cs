using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies
{
    public class MovieQueryHandler :
        IRequestHandler<MovieIdByQuery, MovieResponse>,
        IRequestHandler<GetAllMovieQuery, List<MovieResponse>>
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;
        public MovieQueryHandler(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public async Task<MovieResponse> Handle(MovieIdByQuery request, CancellationToken cancellationToken)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (movie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {request.Id} not found.");
            }
            return await Task.FromResult(mapper.Map<MovieResponse>(movie));
        }
        public async Task<List<MovieResponse>> Handle(GetAllMovieQuery request, CancellationToken cancellationToken)
        {
            var movies = await context.Movies.ToListAsync(cancellationToken);
            if (movies == null)
            {
                throw new KeyNotFoundException($"Movies not found.");
            }
            return await Task.FromResult(mapper.Map<List<MovieResponse>>(movies));
        }

    }
}
