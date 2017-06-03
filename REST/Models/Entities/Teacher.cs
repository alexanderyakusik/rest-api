using System.Collections.Generic;

namespace REST.Models.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        public int ChairId { get; set; }
        public virtual Chair Chair { get; set; }

        public virtual List<Course> Courses { get; set; }
    }
}