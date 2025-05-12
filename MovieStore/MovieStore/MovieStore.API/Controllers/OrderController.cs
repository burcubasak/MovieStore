using MediatR;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Orders;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Order;
using MovieStore.MovieStore.Schema;                   
using System;
using System.Collections.Generic;
using System.Security.Claims; 
using System.Threading;
using System.Threading.Tasks;

namespace MovieStore.MovieStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)] 
        [ProducesResponseType(401)]
        [ProducesResponseType(404)] 
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "User ID could not be determined from token." });
            }

            try
            {
                var command = new CreateOrderCommand(userId, request);
                var orderResponse = await _mediator.Send(command, cancellationToken);
                return StatusCode(201, orderResponse);
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
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetMyOrders(CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "User ID could not be determined from token." });
            }

            var query = new GetOrdersForUserQuery(userId);
            var orders = await _mediator.Send(query, cancellationToken);
            return Ok(orders);
        }
        [HttpGet("{orderId:guid}", Name = "GetOrderById")] 
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(401)] 
        [ProducesResponseType(404)] 
        public async Task<ActionResult<OrderResponse>> GetOrderById(Guid orderId, CancellationToken cancellationToken)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "User ID could not be determined from token." });
            }

            var query = new GetOrderByIdForUserQuery(orderId, userId);
            var order = await _mediator.Send(query, cancellationToken);

            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {orderId} not found for the current user." });
            }

            return Ok(order);
        }
    }
}
