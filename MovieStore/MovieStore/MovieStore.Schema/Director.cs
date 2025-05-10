using System;


namespace MovieStore.MovieStore.Schema
{
    public class DirectorRequest
    {
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }
    public class DirectorResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; }
    }
}
