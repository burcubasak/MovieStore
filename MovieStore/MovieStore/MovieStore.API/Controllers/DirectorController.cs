using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.DirectorImpl.Commands;
using MovieStore.MovieStore.API.Cqrs.DirectorImpl.Queries; 
using MovieStore.MovieStore.Schema; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieStore.MovieStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DirectorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DirectorController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DirectorResponse>), 200)]
        public async Task<ActionResult<IEnumerable<DirectorResponse>>> GetAllDirectors()
        {
            var query = new GetAllDirectorsQuery();
            var directors = await _mediator.Send(query);
            return Ok(directors);
        }
        [HttpGet("{id:guid}", Name = "GetDirectorById")] 
        [ProducesResponseType(typeof(DirectorResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DirectorResponse>> GetDirectorById(Guid id)
        {
            var query = new GetDirectorByIdQuery(id); 
            var director = await _mediator.Send(query);
            if (director == null)
            {
                return NotFound(new { message = $"Director with ID {id} not found." });
            }
            return Ok(director);
        }
        [HttpPost]
        [ProducesResponseType(typeof(DirectorResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<DirectorResponse>> CreateDirector([FromBody] DirectorRequest directorRequest)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            var command = new CreateDirectorCommand(directorRequest); 
            try
            {
                var directorResponse = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetDirectorById), new { id = directorResponse.Id }, directorResponse);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(DirectorResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)] 
        public async Task<IActionResult> UpdateDirector(Guid id, [FromBody] DirectorRequest directorRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var command = new UpdateDirectorCommand(id, directorRequest);
            try
            {
                var updatedDirector = await _mediator.Send(command);
                return Ok(updatedDirector);
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
        [ProducesResponseType(typeof(DirectorResponse), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)] 
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDirector(Guid id)
        {
            var command = new DeleteDirectorCommand(id); 
            try
            {
                var deletedDirector = await _mediator.Send(command);
                return Ok(new { message = $"Director with ID {id} has been deactivated.", director = deletedDirector });
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
    }
}
