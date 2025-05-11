using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.User;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.User;
using MovieStore.MovieStore.Schema;                     
using System;
using System.Threading;
using System.Threading.Tasks;


namespace MovieStore.MovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
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
              
                return StatusCode(201, userResponse);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
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

     
    }
}
