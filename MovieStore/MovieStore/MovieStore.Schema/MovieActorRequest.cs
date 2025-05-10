

namespace MovieStore.MovieStore.Schema
{
    public class AssignActorToMovieRequest
    {
        public Guid ActorId { get; set; }
    }
    public class MovieActorStatusResponse
    {
        public Guid MovieId { get; set; }
        public Guid ActorId { get; set; }
        public bool IsActiveInRole { get; set; } 
    }
}
