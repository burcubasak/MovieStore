using System;
using System.Collections.Generic;

namespace MovieStore.MovieStore.Schema
{

    public class OrderCreateRequest
    {

        public Guid MovieId { get; set; }

    }

    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; } = null!;
        public Guid UserId { get; set; }
        public string CustomerFullName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsActive { get; set; }
    }
}