using MediatR;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;
using MovieStore.MovieStore.API.DbContexts;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies
{
    public class MovieCommandHandler :
        IRequestHandler<CreateMovieCommand, MovieResponse>,
        IRequestHandler<UpdateMovieCommand, MovieResponse>,
        IRequestHandler<DeleteMovieCommand, MovieResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<MovieRequest> _createValidator;

        public MovieCommandHandler(AppDbContext dbContext, IMapper mapper, IValidator<MovieRequest> createValidator)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _createValidator = createValidator;
        }


        public async Task<MovieResponse> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
        {
            // Validate the incoming request
            var validationResult = _createValidator.Validate(request.Movie);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Check if the DirectorId exists in the Directors table
            var directorExists = await _dbContext.Directors
                .AnyAsync(d => d.Id == request.Movie.DirectorId, cancellationToken);

            if (!directorExists)
            {
                throw new KeyNotFoundException($"Director with ID {request.Movie.DirectorId} not found.");
            }

            // Map the request to the Movie entity
            var newMovie = _mapper.Map<Entities.Movie>(request.Movie);

            // Add and save the new movie
            await _dbContext.Movies.AddAsync(newMovie, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return the created movie as a response
            return _mapper.Map<MovieResponse>(newMovie);
        }



        public async Task<MovieResponse> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
        {
            var movie = await _dbContext.Movies.FindAsync(new object[] { request.Id }, cancellationToken);

            if (movie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {request.Id} not found.");
            }

            movie = _mapper.Map<Entities.Movie>(request.Movie);
     

            _dbContext.Movies.Update(movie);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<MovieResponse>(movie);

        }

        public async Task<MovieResponse> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
        {
            var movie = await _dbContext.Movies.FindAsync(new object[] { request.Id }, cancellationToken);

            if (movie == null)
            {
                throw new KeyNotFoundException($"Movie with ID {request.Id} not found.");
            }

            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<MovieResponse>(movie);
        
        }
    }
}
