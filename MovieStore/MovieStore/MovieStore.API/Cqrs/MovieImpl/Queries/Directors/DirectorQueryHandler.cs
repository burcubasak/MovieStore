// Yönetmen sorgularını işleyen handler
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.MovieStore.API.Cqrs.DirectorImpl.Queries;
using MovieStore.MovieStore.API.DbContexts;
using MovieStore.MovieStore.Schema;

public class DirectorQueryHandler :
    IRequestHandler<GetAllDirectorsQuery, IEnumerable<DirectorResponse>>,
    IRequestHandler<GetDirectorByIdQuery, DirectorResponse?>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DirectorQueryHandler(AppDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    public async Task<IEnumerable<DirectorResponse>> Handle(GetAllDirectorsQuery request, CancellationToken cancellationToken)
    {
        var directors = await _context.Directors
                                    .Where(d => d.IsActive) 
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<DirectorResponse>>(directors);
    }
    public async Task<DirectorResponse?> Handle(GetDirectorByIdQuery request, CancellationToken cancellationToken)
    {
        var director = await _context.Directors
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        return _mapper.Map<DirectorResponse>(director);
    }
}