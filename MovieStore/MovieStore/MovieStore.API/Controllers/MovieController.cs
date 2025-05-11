using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries;      
using MovieStore.MovieStore.Schema;                            
using System;
using System.Collections.Generic;
using System.Threading; // CancellationToken için
using System.Threading.Tasks;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Movies;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Movies;
using static MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.MovieActors.MovieActorCommand;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.MoviesActors;

namespace MovieStore.MovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MovieController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        [HttpPost]
        [ProducesResponseType(typeof(MovieResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<IActionResult> CreateMovie([FromBody] MovieRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new CreateMovieCommand(request);
                var movieResponse = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetMovieById), new { id = movieResponse.Id }, movieResponse);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpGet("{id:guid}", Name = "GetMovieById")]
        [ProducesResponseType(typeof(MovieResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMovieById(Guid id, CancellationToken cancellationToken)
        {
            var query = new MovieIdByQuery(id);
            var movie = await _mediator.Send(query, cancellationToken);
            if (movie == null)
            {
                return NotFound(new { message = $"Movie with ID {id} not found." });
            }
            return Ok(movie);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MovieResponse>), 200)]
        public async Task<IActionResult> GetAllMovies(CancellationToken cancellationToken)
        {
            var query = new GetAllMovieQuery();
            var movies = await _mediator.Send(query, cancellationToken);
            return Ok(movies);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MovieResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] MovieRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new UpdateMovieCommand(id, request);
                var updatedMovie = await _mediator.Send(command, cancellationToken);
                return Ok(updatedMovie);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(MovieResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)] 
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteMovie(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteMovieCommand(id);
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("{movieId:guid}/actors")]
        [ProducesResponseType(typeof(MovieActorStatusResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<ActionResult<MovieActorStatusResponse>> AssignActorToMovie(Guid movieId, [FromBody] AssignActorToMovieRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new LinkActorToMovieCommand(movieId, request.ActorId);
                var result = await _mediator.Send(command, cancellationToken);
                return StatusCode(201, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpDelete("{movieId:guid}/actors/{actorId:guid}")]
        [ProducesResponseType(204)] 
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveActorFromMovie(Guid movieId, Guid actorId, [FromQuery] bool hardDelete = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new UnlinkActorFromMovieCommand(movieId, actorId, hardDelete);
                await _mediator.Send(command, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{movieId:guid}/actors")]
        [ProducesResponseType(typeof(IEnumerable<ActorResponse>), 200)]
        [ProducesResponseType(404)] 
        public async Task<ActionResult<IEnumerable<ActorResponse>>> GetActorsForMovie(Guid movieId, CancellationToken cancellationToken)
        {

            var query = new GetActiveActorsForMovieQuery(movieId);
            var actors = await _mediator.Send(query, cancellationToken);
            return Ok(actors); 
        }
    }
}
