using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.User;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.User;
using MovieStore.MovieStore.Schema;                     

namespace MovieStore.MovieStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

       
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)] 
        public async Task<IActionResult> RegisterUser([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var command = new CreateUserCommand(request);
                var userResponse = await _mediator.Send(command, cancellationToken);

                return CreatedAtAction(nameof(GetUserById), new { userId = userResponse.Id }, userResponse);
                
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)] 
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var query = new LoginUserQuery(request);
                var loginResponse = await _mediator.Send(query, cancellationToken);
                return Ok(loginResponse);
            }
            catch (InvalidOperationException ex) 
            {
                return Unauthorized(new { message = ex.Message }); 
            }
        }

        [HttpGet("{userId:guid}", Name = "GetUserById")] 
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserResponse>> GetUserById(Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(userId);
            var user = await _mediator.Send(query, cancellationToken);

            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found." });
            }
            return Ok(user);
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), 200)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var query = new GetAllUsersQuery();
            var users = await _mediator.Send(query, cancellationToken);
            return Ok(users);
        }
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(204)] 
        [ProducesResponseType(typeof(ProblemDetails), 400)] 
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteUserCommand(userId);
                await _mediator.Send(command, cancellationToken);
                return NoContent(); 
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
