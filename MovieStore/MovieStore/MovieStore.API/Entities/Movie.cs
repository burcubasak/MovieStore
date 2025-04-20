using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieStore.MovieStore.API.Entities
{
    public class Movie
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public int Year { get; set; }
        public string Genre { get; set; } = null!;
        public Guid DirectorId { get; set; }
        public decimal Price { get; set; }
        public Director Director { get; set; } = null!;
        public ICollection<MovieActor> MovieActors { get; set; } = new HashSet<MovieActor>();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public ICollection<Actor> Actors { get; set; } = new HashSet<Actor>();
        public bool IsActive { get; set; } = true;
    }



    public class MovieConfiguration:IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Title).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Year).IsRequired();
            builder.Property(m => m.Genre).IsRequired().HasMaxLength(50);
            builder.Property(m => m.Price).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(m => m.IsActive).IsRequired();

            builder.HasOne(m => m.Director)
                .WithMany(d => d.Movies)
                .HasForeignKey(m => m.DirectorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.MovieActors)
                .WithOne(fa => fa.Movie)
                .HasForeignKey(fa => fa.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
