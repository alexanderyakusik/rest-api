namespace REST.Models
{
    using REST.Models.Entities;
    using System.Data.Entity;

    public class UniversityModel : DbContext
    {
        public UniversityModel()
            : base("name=UniversityModel")
        {

        }

        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<Chair> Chairs { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<Course> Courses { get; set; }
    }
}