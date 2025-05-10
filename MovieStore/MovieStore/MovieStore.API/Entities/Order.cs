using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieStore.MovieStore.API.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MovieId { get; set; }
        public string Price { get; set; }=null!;
        public Movie Movie { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.MovieId).IsRequired();
            builder.Property(o => o.OrderDate).IsRequired();
            builder.Property(o => o.Price).IsRequired().HasMaxLength(50);   
            builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(100);
            builder.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(100);
            builder.Property(o => o.IsActive).IsRequired();
            builder.HasOne(o => o.Movie)
       .WithMany(m => m.Orders)
       .HasForeignKey(o => o.MovieId)
       .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
