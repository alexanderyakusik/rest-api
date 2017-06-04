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

            if (id != teacher.Id)
            {
                return BadRequest();
            }

            db.Entry(teacher).State = EntityState.Modified;

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

            if (!TryAttachCourses(teacher))
            {
                return Conflict();
            }

            try
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
                teacher.Chair = db.Chairs.Find(teacher.ChairId);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return Conflict();
            }

            return CreatedAtRoute("DefaultApi", new { id = teacher.Id }, ToPOCO(teacher));
        }

        // DELETE: api/Teachers/5
        [ResponseType(typeof(Teacher))]
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

        private bool TryAttachCourses(Teacher teacher)
        {
            if (teacher.Courses != null)
            {
                List<Course> courses = new List<Course>();

                foreach (Course course in teacher.Courses)
                {
                    Course attachedCourse = db.Courses.Find(course.Id);
                    if (attachedCourse == null)
                    {
                        return false;
                    }
                    courses.Add(attachedCourse);
                }

                teacher.Courses = courses;
            }

            return true;
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