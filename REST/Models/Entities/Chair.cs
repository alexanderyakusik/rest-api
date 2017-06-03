using System.Collections.Generic;

namespace REST.Models.Entities
{
    public class Chair
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int FacultyId { get; set; }
        public virtual Faculty Faculty { get; set; }

        public virtual List<Teacher> Teachers { get; set; }
    }
}