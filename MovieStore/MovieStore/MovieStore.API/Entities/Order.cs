using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations.Schema; 

namespace MovieStore.MovieStore.API.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string PurchasedMovieTitle { get; set; } = null!;

        public string CustomerFullName { get; set; } = null!; 

        public decimal Price { get; set; } 

        public DateTime OrderDate { get; set; } = DateTime.UtcNow; 

        public bool IsActive { get; set; } = true; 
    }

    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.MovieId).IsRequired();
            builder.Property(o => o.UserId).IsRequired();

            builder.Property(o => o.PurchasedMovieTitle)
                   .IsRequired()
                   .HasMaxLength(200); 

            builder.Property(o => o.CustomerFullName) 
                   .IsRequired()
                   .HasMaxLength(100); 

            builder.Property(o => o.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)"); 

            builder.Property(o => o.OrderDate).IsRequired();
            builder.Property(o => o.IsActive).IsRequired();

            builder.HasOne(o => o.Movie)
                   .WithMany(m => m.Orders) 
                   .HasForeignKey(o => o.MovieId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders) 
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
