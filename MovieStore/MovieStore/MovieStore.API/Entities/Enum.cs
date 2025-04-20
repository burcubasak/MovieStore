namespace MovieStore.MovieStore.API.Entities
{
    public class Enum
    {
        public enum MovieGenre
        {
            Action = 1,
            Comedy,
            Drama,
            Horror,
            SciFi,
            Romance,
            Thriller,
            Documentary,
            Animation,
            Adventure
        }
        public enum MovieStatus
        {
            Active,
            Inactive
        }
        public enum ActorStatus
        {
            Active,
            Inactive
        }
        public enum DirectorStatus
        {
            Active,
            Inactive
        }
    }
}
