using System.Collections.Generic;

namespace SchemaValidation.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public double Age { get; set; }
        public bool IsActive { get; set; }
        public List<string> Tags { get; set; }
        public Address Address { get; set; }
    }
} 