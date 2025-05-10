using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using static MovieStore.MovieStore.API.Entities.Enum; 
namespace MovieStore.MovieStore.API.Entities 
{
    public class Movie
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public int Year { get; set; }
        public MovieGenre Genre { get; set; }
        public Guid DirectorId { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public Director Director { get; set; } = null!;
        public ICollection<MovieActor> MovieActors { get; set; } = new HashSet<MovieActor>();
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public ICollection<Actor> Actors { get; set; } = new HashSet<Actor>();
    }

    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Title)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(m => m.Year)
                   .IsRequired();
            builder.Property(m => m.Genre)
                   .IsRequired();
            builder.Property(m => m.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)"); 

            builder.Property(m => m.IsActive)
                   .IsRequired();
           
            builder.HasOne(m => m.Director)
                   .WithMany(d => d.Movies)
                   .HasForeignKey(m => m.DirectorId)
                   .OnDelete(DeleteBehavior.Restrict); 
            builder.HasMany(m => m.MovieActors)
                   .WithOne(ma => ma.Movie) 
                   .HasForeignKey(ma => ma.MovieId)
                   .OnDelete(DeleteBehavior.Restrict); 
            builder.HasMany(m => m.Orders)
                   .WithOne(o => o.Movie) 
                   .HasForeignKey(o => o.MovieId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
