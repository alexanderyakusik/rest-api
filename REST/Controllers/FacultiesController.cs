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
    public class FacultiesController : ApiController
    {
        private UniversityModel db = new UniversityModel();

        // GET: api/Faculties
        public IQueryable<Faculty> GetFaculties()
        {
            List<Faculty> faculties = db.Faculties.ToList();
            List<Faculty> resultList = faculties.Select((Faculty f) => ToPOCO(f)).ToList();

            return resultList.AsQueryable();
        }

        // GET: api/Faculties/5
        [ResponseType(typeof(Faculty))]
        public IHttpActionResult GetFaculty(int id)
        {
            Faculty faculty = db.Faculties.Find(id);
            if (faculty == null)
            {
                return NotFound();
            }

            return Ok(ToPOCO(faculty));
        }

        // PUT: api/Faculties/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutFaculty(int id, Faculty faculty)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (faculty == null)
            {
                return BadRequest();
            }

            UpdateFaculty(faculty, id);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FacultyExists(id))
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

        // POST: api/Faculties
        [ResponseType(typeof(Faculty))]
        public IHttpActionResult PostFaculty(Faculty faculty)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((faculty == null) || !ChairsExist(faculty))
            {
                return BadRequest();
            }

            AttachChairs(faculty);

            try
            {
                db.Faculties.Add(faculty);
                db.SaveChanges();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is DbUpdateException)
            {
                return Conflict();
            }

            return CreatedAtRoute("DefaultApi", new { id = faculty.Id }, ToPOCO(faculty));
        }

        // DELETE: api/Faculties/5
        [ResponseType(typeof(void))]
        public IHttpActionResult DeleteFaculty(int id)
        {
            Faculty faculty = db.Faculties.Find(id);
            if (faculty == null)
            {
                return NotFound();
            }

            db.Faculties.Remove(faculty);
            db.SaveChanges();

            return Ok();
        }

        private Faculty ToPOCO(Faculty faculty)
        {
            List<Chair> chairs = null;
            if (faculty.Chairs != null)
            {
                chairs = faculty.Chairs.Select((Chair c) => new Chair()
                {
                    Id = c.Id,
                    FacultyId = c.FacultyId,
                    Faculty = null,
                    Name = c.Name,
                    Teachers = null
                }).ToList();
            }

            return new Faculty()
            {
                Id = faculty.Id,
                Name = faculty.Name,
                Chairs = chairs
            };
        }

        private void UpdateChairs(Faculty updatingFaculty, Faculty faculty)
        {
            List<Chair> oldChairs = updatingFaculty.Chairs.ToList();

            if (faculty.Chairs == null)
            {
                return;
            }

            List<Chair> chairs = new List<Chair>();
            faculty.Chairs.ForEach( (Chair c) => chairs.Add(db.Chairs.Find(c.Id)) );

            updatingFaculty.Chairs.Clear();
            foreach (Chair chair in chairs)
            {
                updatingFaculty.Chairs.Add(chair);
            }

            oldChairs = oldChairs.Except(updatingFaculty.Chairs).ToList();
            oldChairs.ForEach((Chair c) => db.Entry(c).State = EntityState.Deleted);
        }

        private void UpdateFaculty(Faculty faculty, int id)
        {
            Faculty updatingFaculty = db.Faculties.Include("Chairs").Single((Faculty f) => f.Id == id);
            UpdateChairs(updatingFaculty, faculty);
            if (faculty.Name != null)
            {
                updatingFaculty.Name = faculty.Name;
            }
        }

        private void AttachChairs(Faculty faculty)
        {
            if (faculty.Chairs == null)
            {
                return;
            }

            List<Chair> chairs = new List<Chair>();
            foreach (Chair chair in faculty.Chairs)
            {
                chairs.Add(db.Chairs.Find(chair.Id));
            }

            faculty.Chairs = chairs;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FacultyExists(int id)
        {
            return db.Faculties.Count(e => e.Id == id) > 0;
        }

        private bool ChairsExist(Faculty faculty)
        {
            if (faculty.Chairs == null)
            {
                return true;
            }

            foreach (Chair chair in faculty.Chairs)
            {
                if (db.Chairs.Find(chair.Id) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}