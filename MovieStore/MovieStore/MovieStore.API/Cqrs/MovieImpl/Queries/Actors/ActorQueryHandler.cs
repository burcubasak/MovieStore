using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Queries.Actors
{
    public class ActorQueryHandler:
        IRequestHandler<ActorIdByQuery, ActorResponse>,
        IRequestHandler<GetAllActorQuery, List<ActorResponse>>
    {

        private readonly AppDbContext context;
        private readonly IMapper mapper;
        public ActorQueryHandler(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public async  Task<ActorResponse> Handle(ActorIdByQuery request, CancellationToken cancellationToken)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (actor == null)
            {
                throw new KeyNotFoundException($"Actor with ID {request.Id} not found.");
            }
            return await Task.FromResult(mapper.Map<ActorResponse>(actor));

        }
        public async Task<List<ActorResponse>> Handle(GetAllActorQuery request, CancellationToken cancellationToken)
        {
            var actors = await context.Actors.ToListAsync(cancellationToken);

            if (actors == null)
            {
                throw new KeyNotFoundException($"Actors not found.");
            }
            return await Task.FromResult(mapper.Map<List<ActorResponse>>(actors));
        }
    }
  
}
