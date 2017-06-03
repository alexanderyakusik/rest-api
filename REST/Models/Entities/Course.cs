using System.Collections.Generic;

namespace REST.Models.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<Teacher> Teachers { get; set; }
    }
}