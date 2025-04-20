using MovieStore.MovieStore.Base.Schema;

namespace MovieStore.MovieStore.Schema
{
    public class MovieRequest
    {
        public string Title { get; set; } = null!;
        public int Year { get; set; }
        public string Genre { get; set; } = null!;
        public Guid DirectorId { get; set; }
        public decimal Price { get; set; }
    }

    public class MovieResponse:BaseResponse
    {
     
    
    }
}