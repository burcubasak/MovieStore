using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.DirectorImpl.Commands;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.API.Entities;
using MovieStore.MovieStore.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MovieStore.MovieStore.API.Cqrs.DirectorImpl.Handlers
{
    public class DirectorCommandHandler :
        IRequestHandler<CreateDirectorCommand, DirectorResponse>,
        IRequestHandler<UpdateDirectorCommand, DirectorResponse>,
        IRequestHandler<DeleteDirectorCommand, DirectorResponse>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DirectorCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<DirectorResponse> Handle(CreateDirectorCommand request, CancellationToken cancellationToken)
        {
            var existingDirector = await _context.Directors
                .FirstOrDefaultAsync(d => d.Name == request.Director.Name && d.SurName == request.Director.SurName, cancellationToken);

            if (existingDirector != null && existingDirector.IsActive) 
            {
                throw new InvalidOperationException($"An active director with the name {request.Director.Name} and surname {request.Director.SurName} already exists.");
            }

            var newDirector = _mapper.Map<Director>(request.Director);
            newDirector.IsActive = true; 

            await _context.Directors.AddAsync(newDirector, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<DirectorResponse>(newDirector);
        }
        public async Task<DirectorResponse> Handle(UpdateDirectorCommand request, CancellationToken cancellationToken)
        {
            var directorToUpdate = await _context.Directors.FindAsync(new object[] { request.Id }, cancellationToken);

            if (directorToUpdate == null || !directorToUpdate.IsActive)
            {
                throw new KeyNotFoundException($"Active director with ID {request.Id} not found.");
            }
            var conflictingDirector = await _context.Directors
                .FirstOrDefaultAsync(d => d.Id != request.Id &&
                                          d.Name == request.Director.Name &&
                                          d.SurName == request.Director.SurName &&
                                          d.IsActive, cancellationToken);

            if (conflictingDirector != null)
            {
                throw new InvalidOperationException($"Another active director with the name {request.Director.Name} and surname {request.Director.SurName} already exists.");
            }

            _mapper.Map(request.Director, directorToUpdate);
            _context.Directors.Update(directorToUpdate);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<DirectorResponse>(directorToUpdate);
        }
        public async Task<DirectorResponse> Handle(DeleteDirectorCommand request, CancellationToken cancellationToken)
        {
            var directorToDelete = await _context.Directors.FindAsync(new object[] { request.Id }, cancellationToken);

            if (directorToDelete == null)
            {
                throw new KeyNotFoundException($"Director with ID {request.Id} not found.");
            }

            if (!directorToDelete.IsActive)
            {
                throw new InvalidOperationException($"Director with ID {request.Id} is already inactive.");
            }
            directorToDelete.IsActive = false;
            _context.Directors.Update(directorToDelete);
            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<DirectorResponse>(directorToDelete);
        }
    }

   
}
