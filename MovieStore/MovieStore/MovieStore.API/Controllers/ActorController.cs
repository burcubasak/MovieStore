using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors;

namespace MovieStore.MovieStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActorController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ActorController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllActor([FromQuery] GetAllActorQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new ActorIdByQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateActor([FromBody] CreateActorCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(ActorIdByQuery), new { id = result.Id }, result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActor([FromRoute] Guid id, [FromBody] UpdateActorCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActor([FromRoute] Guid id, [FromBody] DeleteActorCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
 
}
