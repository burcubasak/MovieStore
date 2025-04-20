using MovieStore.MovieStore.Base.Schema;

namespace MovieStore.MovieStore.Schema
{
    public class ActorRequest 
    {
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

    }

    public class ActorResponse : BaseResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Guid MovieId { get; set; }
    }
}
