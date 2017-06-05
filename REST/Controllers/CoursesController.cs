using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using REST.Models;
using REST.Models.Entities;

namespace REST.Controllers
{
    public class CoursesController : ApiController
    {
        private UniversityModel db = new UniversityModel();

        // GET: api/Courses
        public IQueryable<Course> GetCourses()
        {
            List<Course> courseList = db.Courses.ToList();
            List<Course> resultList = courseList.Select((Course course) => ToPOCO(course)).ToList();

            return resultList.AsQueryable();
        }

        // GET: api/Courses/5
        [ResponseType(typeof(Course))]
        public IHttpActionResult GetCourse(int id)
        {
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }

            return Ok(ToPOCO(course));
        }

        // PUT: api/Courses/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCourse(int id, Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != course.Id)
            {
                return BadRequest();
            }

            db.Entry(course).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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

        // POST: api/Courses
        [ResponseType(typeof(Course))]
        public IHttpActionResult PostCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryAttachTeachers(course))
            {
                return Conflict();
            }

            try
            {
                db.Courses.Add(course);
                db.SaveChanges();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return Conflict();
            }

            return CreatedAtRoute("DefaultApi", new { id = course.Id }, ToPOCO(course));
        }

        // DELETE: api/Courses/5
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteCourse(int id)
        {
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }

            db.Courses.Remove(course);
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

        private bool CourseExists(int id)
        {
            return db.Courses.Count(e => e.Id == id) > 0;
        }

        private bool TryAttachTeachers(Course course)
        {
            if (course.Teachers != null)
            {
                List<Teacher> teachers = new List<Teacher>();

                foreach (Teacher teacher in course.Teachers)
                {
                    Teacher attachedTeacher = db.Teachers.Find(teacher.Id);
                    if (attachedTeacher == null)
                    {
                        return false;
                    }
                    teachers.Add(attachedTeacher);
                }

                course.Teachers = teachers;
            }

            return true;
        }

        private Course ToPOCO(Course course)
        {
            List<Teacher> teachersList = null;
            if (course.Teachers != null)
            {
                teachersList = course.Teachers.Select((Teacher teacher) => new Teacher()
                {
                    Id = teacher.Id,
                    FullName = teacher.FullName,
                    ChairId = teacher.ChairId,
                    Chair = null,
                    Courses = null
                }).ToList();
            }

            return new Course()
            {
                Id = course.Id,
                Name = course.Name,
                Teachers = teachersList
            };
        }
    }
}