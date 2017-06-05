using REST.Models;
using REST.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace REST.Controllers
{
    public class TeachersController : ApiController
    {
        private UniversityModel db = new UniversityModel();

        // GET: api/Teachers
        public IQueryable<Teacher> GetTeachers()
        {
            List<Teacher> teacherList = db.Teachers.ToList();
            List<Teacher> resultList = teacherList.Select((Teacher teacher) => ToPOCO(teacher)).ToList();

            return resultList.AsQueryable();
        }

        // GET: api/Teachers/5
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult GetTeacher(int id)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return Ok(ToPOCO(teacher));
        }

        // PUT: api/Teachers/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTeacher(int id, Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (teacher == null)
            {
                return BadRequest();
            }

            UpdateTeacher(teacher, id);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Teachers
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult PostTeacher(Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((teacher == null) || !ChairExists(teacher) || !CoursesExist(teacher))
            {
                return BadRequest();
            }

            AttachChair(teacher);
            AttachCourses(teacher);

            try
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return BadRequest();
            }

            return CreatedAtRoute("DefaultApi", new { id = teacher.Id }, ToPOCO(teacher));
        }

        // DELETE: api/Teachers/5
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteTeacher(int id)
        {
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }

            db.Teachers.Remove(teacher);
            db.SaveChanges();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TeacherExists(int id)
        {
            return db.Teachers.Count(e => e.Id == id) > 0;
        }

        private void AttachCourses(Teacher teacher)
        {
            if (teacher.Courses == null)
            {
                return;
            }

            List<Course> courses = new List<Course>();
            foreach (Course course in teacher.Courses)
            {
                courses.Add(db.Courses.Find(course.Id));
            }

            teacher.Courses = courses;
        }

        private void AttachChair(Teacher teacher)
        {
            teacher.Chair = db.Chairs.Find(teacher.ChairId);
        }

        private bool ChairExists(Teacher teacher)
        { 
            if (db.Chairs.Find(teacher.ChairId) == null)
            {
                return false;
            }

            return true;
        }

        private bool CoursesExist(Teacher teacher)
        {
            if (teacher.Courses == null)
            {
                return true;
            }

            foreach (Course course in teacher.Courses)
            {
                if (db.Courses.Find(course.Id) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateCourses(Teacher updatingTeacher, Teacher teacher)
        {
            if (teacher.Courses == null)
            {
                updatingTeacher.Courses.Clear();
                return;
            }

            List<Course> courses = new List<Course>();
            teacher.Courses.ForEach( (Course c) => courses.Add(db.Courses.Find(c.Id)) );

            updatingTeacher.Courses.Clear();
            foreach (Course course in courses)
            {
                updatingTeacher.Courses.Add(course);
            }
        }

        private void UpdateTeacher(Teacher teacher, int id)
        {
            Teacher updatingTeacher = db.Teachers.Include("Courses").Single((Teacher t) => t.Id == id);
            UpdateCourses(updatingTeacher, teacher);
            if (teacher.FullName != null)
            {
                updatingTeacher.FullName = teacher.FullName;
            }
            if (teacher.ChairId != 0)
            {
                updatingTeacher.ChairId = teacher.ChairId;
            }   
        }

        private Teacher ToPOCO(Teacher teacher)
        {
            Chair teacherChair = null;
            if (teacher.Chair != null)
            {
                teacherChair = new Chair();
                teacherChair.Id = teacher.Chair.Id;
                teacherChair.Name = teacher.Chair.Name;
                teacherChair.FacultyId = teacher.Chair.FacultyId;
                teacherChair.Faculty = null;
                teacherChair.Teachers = null;
            }

            List<Course> coursesList = null;
            if (teacher.Courses != null)
            {
                coursesList = teacher.Courses.Select((Course course) => new Course()
                {
                    Id = course.Id,
                    Name = course.Name,
                    Teachers = null
                }).ToList();
            }

            return new Teacher()
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                ChairId = teacher.ChairId,
                Chair = teacherChair,
                Courses = coursesList
            };
        }
    }
}