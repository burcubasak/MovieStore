using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors; 
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors;  
using MovieStore.MovieStore.Schema;                           
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries;

namespace MovieStore.MovieStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActorController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ActorController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ActorResponse>), 200)]
        public async Task<IActionResult> GetAllActors(CancellationToken cancellationToken)
        {
            var query = new GetAllActorQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id:guid}", Name = "GetActorById")]
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActorById(Guid id, CancellationToken cancellationToken)
        {
            var query = new ActorIdByQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = $"Actor with ID {id} not found." });
            }
            return Ok(result);
        }
        [HttpPost]
        [ProducesResponseType(typeof(ActorResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<IActionResult> CreateActor([FromBody] ActorRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new CreateActorCommand(request);
                var result = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetActorById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<IActionResult> UpdateActor(Guid id, [FromBody] ActorRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var command = new UpdateActorCommand(id, request);
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(result);
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
        [ProducesResponseType(typeof(ActorResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)] 
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActor(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteActorCommand(id);
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
        [HttpGet("{actorId:guid}/movies")]
        [ProducesResponseType(typeof(IEnumerable<MovieResponse>), 200)]
        [ProducesResponseType(404)] 
        public async Task<ActionResult<IEnumerable<MovieResponse>>> GetMoviesForActor(Guid actorId, CancellationToken cancellationToken)
        {
            var query = new GetActiveMoviesForActorQuery(actorId);
            var movies = await _mediator.Send(query, cancellationToken);
            return Ok(movies);
        }
    }
}
