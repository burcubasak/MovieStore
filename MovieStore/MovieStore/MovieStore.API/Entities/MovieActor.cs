using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieStore.MovieStore.API.Entities
{
    public class MovieActor
    {
        public Guid MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public Guid ActorId { get; set; }
        public Actor Actor { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }

    public class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
    {
        public void Configure(EntityTypeBuilder<MovieActor> builder)
        {
            builder.HasKey(ma => new { ma.MovieId, ma.ActorId });
            builder.Property(ma => ma.IsActive).IsRequired();
            builder.HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
