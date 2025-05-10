using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors;
using MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors; // ActorIdByQuery için using
using MovieStore.MovieStore.Schema; // ActorResponse için (varsayım)

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

        // Bu metodun adını CreatedAtAction'da kullanacağız
        [HttpGet("{id:guid}", Name = "GetActorById")] // Name özelliği eklemek iyi bir pratiktir, ancak nameof da çalışır
        public async Task<IActionResult> GetById(Guid id)
        {
            // ActorIdByQuery query sınıfınızın adıyla eşleşiyor, bu doğru.
            var query = new ActorIdByQuery(id);
            var result = await _mediator.Send(query);
            if (result == null) // Varsayım: result null ise NotFound dön
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateActor([FromBody] CreateActorCommand command)
        {
            // Varsayalım ki command handler'ınız ActorResponse veya benzeri bir Id içeren nesne döndürüyor.
            var result = await _mediator.Send(command);

            // DÜZELTME: nameof(GetById) kullanılmalı
            // result.Id'nin yeni oluşturulan aktörün Guid ID'si olduğunu varsayıyoruz.
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")] // Rota parametresine :guid eklemek iyi bir pratiktir
        public async Task<IActionResult> UpdateActor([FromRoute] Guid id, [FromBody] UpdateActorCommand command)
        {
            // ID'nin command içindekiyle eşleştiğini kontrol etmek iyi bir pratiktir.
            // Command nesnenizin bir Id özelliği olduğunu varsayıyorum.
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }
            var result = await _mediator.Send(command);
            if (result == null) // Varsayım: Güncelleme başarısızsa veya kaynak bulunamadıysa
            {
                return NotFound();
            }
            return Ok(result); // Veya NoContent() eğer bir şey döndürmek istemiyorsanız
        }

        [HttpDelete("{id:guid}")] // Rota parametresine :guid eklemek iyi bir pratiktir
        public async Task<IActionResult> DeleteActor([FromRoute] Guid id) // DeleteActorCommand'ı body'den almak yerine ID'yi kullanmak daha yaygındır
        {
            var command = new DeleteActorCommand(id); // Command'ı burada oluşturun
            var result = await _mediator.Send(command);
            if (result == null) // Varsayım: Silme başarısızsa veya kaynak bulunamadıysa
            {
                return NotFound();
            }
            return Ok(result); // Veya NoContent()
        }

    }
}
