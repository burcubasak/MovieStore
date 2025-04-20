using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.MovieImpl.Commands.Actors
{
    public class ActorCommandHandler:
        IRequestHandler<CreateActorCommand, ActorResponse>,
        IRequestHandler<UpdateActorCommand, ActorResponse>,
        IRequestHandler<DeleteActorCommand, ActorResponse>
    {

        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public ActorCommandHandler(AppDbContext context, IMapper mapper) 
        {
            this.context = context;
            this.mapper = mapper;
        
        }

        public async Task<ActorResponse> Handle(CreateActorCommand request, CancellationToken cancellationToken)
        {
            var createActor = await context.Actors
                .FirstOrDefaultAsync(x => x.Name == request.Actor.Name && x.SurName == request.Actor.SurName, cancellationToken);
            if (createActor != null)
            {
                throw new InvalidOperationException($"Actor with name {request.Actor.Name} and surname {request.Actor.SurName} already exists.");
            }
            var newActor = mapper.Map<Actor>(request.Actor);
            await context.Actors.AddAsync(newActor, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<ActorResponse>(newActor);
        }



        public async Task<ActorResponse> Handle(UpdateActorCommand request, CancellationToken cancellationToken)
        {
            // Veritabanında güncellenecek aktörü bul
            var updateActor = await context.Actors.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (updateActor == null)
            {
                throw new KeyNotFoundException($"Actor with ID {request.Id} not found.");
            }

            // Mapper ile güncelleme işlemi
            mapper.Map(request.Actor, updateActor);

            // Veritabanında güncelleme işlemini gerçekleştir
            context.Actors.Update(updateActor);
            await context.SaveChangesAsync(cancellationToken);

            // Güncellenmiş aktörü ActorResponse'a map et ve döndür
            return  mapper.Map<ActorResponse>(updateActor);
        }


        public async Task<ActorResponse> Handle(DeleteActorCommand request, CancellationToken cancellationToken)
        {

            var deleteActor = await context.Actors.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if(deleteActor==null)
            {
                throw new KeyNotFoundException($"Actor with ID {request.Id} not found.");
            }

            deleteActor.IsActive = false;
            context.Actors.Update(deleteActor);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<ActorResponse>(deleteActor);

        }
    }
}
