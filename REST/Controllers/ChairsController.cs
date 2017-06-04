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

            if (id != chair.Id)
            {
                return BadRequest();
            }

            db.Entry(chair).State = EntityState.Modified;

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

            if (!TryAttachTeachers(chair))
            {
                return Conflict();
            }

            try
            {
                db.Chairs.Add(chair);
                db.SaveChanges();
                chair.Faculty = db.Faculties.Find(chair.FacultyId);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return Conflict();
            }

            return CreatedAtRoute("DefaultApi", new { id = chair.Id }, ToPOCO(chair));
        }

        // DELETE: api/Chairs/5
        [ResponseType(typeof(Chair))]
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

        private bool TryAttachTeachers(Chair chair)
        {
            if (chair.Teachers != null)
            {
                List<Teacher> teachers = new List<Teacher>();

                foreach (Teacher teacher in chair.Teachers)
                {
                    Teacher attachedTeacher = db.Teachers.Find(teacher.Id);
                    if (attachedTeacher == null)
                    {
                        return false;
                    }
                    teachers.Add(attachedTeacher);
                }

                chair.Teachers = teachers;
            }

            return true;
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

            return new Chair()
            {
                Id = chair.Id,
                Name = chair.Name,
                FacultyId = chair.FacultyId,
                Faculty = chair.Faculty,
                Teachers = teachersList
            };
        }
    }
}