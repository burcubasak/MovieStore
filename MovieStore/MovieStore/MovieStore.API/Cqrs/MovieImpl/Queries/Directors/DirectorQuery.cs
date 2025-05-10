using MediatR;
using MovieStore.MovieStore.Schema; // DirectorResponse DTO'nuzun namespace'i
using System;
using System.Collections.Generic;

namespace MovieStore.MovieStore.API.Cqrs.DirectorImpl.Queries
{
    public record GetAllDirectorsQuery() : IRequest<IEnumerable<DirectorResponse>>;
    public record GetDirectorByIdQuery(Guid Id) : IRequest<DirectorResponse?>;
}
