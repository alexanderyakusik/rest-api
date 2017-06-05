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
    public class ChairsController : ApiController
    {
        private UniversityModel db = new UniversityModel();

        // GET: api/Chairs
        public IQueryable<Chair> GetChairs()
        {
            List<Chair> chairList = db.Chairs.ToList();
            List<Chair> resultList = chairList.Select((Chair chair) => ToPOCO(chair)).ToList();

            return resultList.AsQueryable();
        }

        // GET: api/Chairs/5
        [ResponseType(typeof(Chair))]
        public IHttpActionResult GetChair(int id)
        {
            Chair chair = db.Chairs.Find(id);
            if (chair == null)
            {
                return NotFound();
            }

            return Ok(ToPOCO(chair));
        }

        // PUT: api/Chairs/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutChair(int id, Chair chair)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (chair == null)
            {
                return BadRequest();
            }

            UpdateChair(chair, id);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChairExists(id))
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

        // POST: api/Chairs
        [ResponseType(typeof(Chair))]
        public IHttpActionResult PostChair(Chair chair)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((chair == null) || !FacultyExists(chair) || !TeachersExist(chair))
            {
                return BadRequest();
            }

            AttachFaculty(chair);
            AttachTeachers(chair);

            try
            {
                db.Chairs.Add(chair);
                db.SaveChanges();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return BadRequest();
            }

            return CreatedAtRoute("DefaultApi", new { id = chair.Id }, ToPOCO(chair));
        }

        // DELETE: api/Chairs/5
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteChair(int id)
        {
            Chair chair = db.Chairs.Find(id);
            if (chair == null)
            {
                return NotFound();
            }

            db.Chairs.Remove(chair);
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

        private bool ChairExists(int id)
        {
            return db.Chairs.Count(e => e.Id == id) > 0;
        }

        private bool TeachersExist(Chair chair)
        {
            if (chair.Teachers == null)
            {
                return true;
            }

            foreach (Teacher teacher in chair.Teachers)
            {
                if (db.Teachers.Find(teacher.Id) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool FacultyExists(Chair chair)
        {
            if (db.Faculties.Find(chair.FacultyId) == null)
            {
                return false;
            }

            return true;
        }

        private void AttachTeachers(Chair chair)
        {
            if (chair.Teachers == null)
            {
                return;
            }

            List<Teacher> teachers = new List<Teacher>();
            foreach (Teacher teacher in chair.Teachers)
            {
                teachers.Add(db.Teachers.Find(teacher.Id));
            }

            chair.Teachers = teachers;
        }

        private void AttachFaculty(Chair chair)
        {
            chair.Faculty = db.Faculties.Find(chair.FacultyId);
        }

        private void UpdateTeachers(Chair updatingChair, Chair chair)
        {
            List<Teacher> oldTeachers = updatingChair.Teachers.ToList();

            if (chair.Teachers == null)
            {
                return;
            }

            List<Teacher> teachers = new List<Teacher>();
            chair.Teachers.ForEach( (Teacher t) => teachers.Add(db.Teachers.Find(t.Id)) );

            updatingChair.Teachers.Clear();
            foreach (Teacher teacher in teachers)
            {
                updatingChair.Teachers.Add(teacher);
            }

            oldTeachers = oldTeachers.Except(updatingChair.Teachers).ToList();
            oldTeachers.ForEach((Teacher t) => db.Entry(t).State = EntityState.Deleted);
        }

        private void UpdateChair(Chair chair, int id)
        {
            Chair updatingChair = db.Chairs.Include("Teachers").Single((Chair c) => c.Id == id);
            UpdateTeachers(updatingChair, chair);
            if (chair.FacultyId != 0)
            {
                updatingChair.FacultyId = chair.FacultyId;
            }
            if (chair.Name != null)
            {
                updatingChair.Name = chair.Name;
            }
        }

        private Chair ToPOCO(Chair chair)
        {
            List<Teacher> teachersList = null;
            if (chair.Teachers != null)
            {
                teachersList = chair.Teachers.Select((Teacher teacher) => new Teacher()
                {
                    Id = teacher.Id,
                    FullName = teacher.FullName,
                    Chair = null,
                    ChairId = teacher.ChairId,
                    Courses = null
                }).ToList();
            }

            Faculty faculty = null;
            if (chair.Faculty != null)
            {
                faculty = new Faculty()
                {
                    Id = chair.Faculty.Id,
                    Name = chair.Faculty.Name,
                    Chairs = null
                };
            }

            return new Chair()
            {
                Id = chair.Id,
                Name = chair.Name,
                FacultyId = chair.FacultyId,
                Faculty = faculty,
                Teachers = teachersList
            };
        }
    }
}