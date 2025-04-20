namespace MovieStore.MovieStore.Base.Domain
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public void MarkAsDeleted()
        {
            IsActive = false;
            UpdatedDate = DateTime.UtcNow;
        }
        public void Update()
        {
            UpdatedDate = DateTime.UtcNow;
        }
    }
}
