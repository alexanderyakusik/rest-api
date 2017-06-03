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
            return db.Chairs;
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

            return Ok(chair);
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

            db.Chairs.Add(chair);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = chair.Id }, chair);
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

            return Ok(chair);
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
    }
}