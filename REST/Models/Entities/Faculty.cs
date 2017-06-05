using System.Collections.Generic;

namespace REST.Models.Entities
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual List<Chair> Chairs { get; set; }
    }
}