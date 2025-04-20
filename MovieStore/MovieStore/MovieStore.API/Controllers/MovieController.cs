using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<MovieRequest> _createValidator;
        private readonly IValidator<MovieRequest> _updateValidator;

        public MovieController(AppDbContext dbContext, IMapper mapper, IValidator<MovieRequest> createValidator, IValidator<MovieRequest> updateValidator)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovie([FromBody] MovieRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _createValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var newMovie = _mapper.Map<Entities.Movie>(request);
            await _dbContext.Movies.AddAsync(newMovie, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetMovieById), new { id = newMovie.Id }, _mapper.Map<MovieResponse>(newMovie));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(Guid id, CancellationToken cancellationToken)
        {
            var movie = await _dbContext.Movies.FindAsync(new object[] { id }, cancellationToken);
            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found.");
            }

            return Ok(_mapper.Map<MovieResponse>(movie));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies(CancellationToken cancellationToken)
        {
            var movies = await _dbContext.Movies.ToListAsync(cancellationToken);
            return Ok(_mapper.Map<List<MovieResponse>>(movies));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] MovieRequest request, CancellationToken cancellationToken)
        {
            var validationResult = _updateValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var movie = await _dbContext.Movies.FindAsync(new object[] { id }, cancellationToken);
            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found.");
            }

            _mapper.Map(request, movie);
            _dbContext.Movies.Update(movie);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id, CancellationToken cancellationToken)
        {
            var movie = await _dbContext.Movies.FindAsync(new object[] { id }, cancellationToken);
            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found.");
            }

            _dbContext.Movies.Remove(movie);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
