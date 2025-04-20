using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieStore.MovieStore.API.Entities
{
    public class Actor
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public ICollection<Movie> Movies { get; set; } = new HashSet<Movie>();     
        public ICollection<MovieActor> MovieActors { get; set; } = new HashSet<MovieActor>();
        public bool IsActive { get; set; } = true;

    }

    public class ActorConfiguration : IEntityTypeConfiguration<Actor>
    {
        public void Configure(EntityTypeBuilder<Actor> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(50);
            builder.Property(a => a.SurName).IsRequired().HasMaxLength(50);
            builder.Property(a => a.DateOfBirth).IsRequired();
            builder.Property(a => a.IsActive).IsRequired();
            builder.HasMany(a => a.MovieActors)
                .WithOne(ma => ma.Actor)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Movies)
         .WithMany(m => m.Actors)
         .UsingEntity<MovieActor>(
             j => j.HasOne(ma => ma.Movie)
                   .WithMany(m => m.MovieActors)
                   .HasForeignKey(ma => ma.MovieId),
             j => j.HasOne(ma => ma.Actor)
                   .WithMany(a => a.MovieActors)
                   .HasForeignKey(ma => ma.ActorId));

        }
    }
}
