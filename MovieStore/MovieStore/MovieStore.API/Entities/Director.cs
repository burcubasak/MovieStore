using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MovieStore.MovieStore.API.Entities
{
    public class Director
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string SurName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public ICollection<Movie> Movies { get; set; } = new HashSet<Movie>();
        public bool IsActive { get; set; } = true;
    }

    public class DirectorConfiguration : IEntityTypeConfiguration<Director>
    {
        public void Configure(EntityTypeBuilder<Director> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(50);
            builder.Property(d => d.SurName).IsRequired().HasMaxLength(50);
            builder.Property(d => d.DateOfBirth).IsRequired();
            builder.Property(d => d.IsActive).IsRequired();
            builder.HasMany(d => d.Movies)
                .WithOne(m => m.Director)
                .HasForeignKey(m => m.DirectorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}