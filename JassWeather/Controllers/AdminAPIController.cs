using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using JassWeather.Models;

namespace JassWeather.Controllers
{
    public class AdminController : ApiController
    {
        private JassWeatherContext db = new JassWeatherContext();
        // GET api/AdminAPI
        public IEnumerable<JassBuilder> GetJassBuilders()
        {
            var jassbuilders = db.JassBuilders.Include(j => j.JassVariable).Include(j => j.JassGrid).Include(j => j.APIRequest);
            return jassbuilders.AsEnumerable();
        }

        // GET api/AdminAPI
        public IEnumerable<JassBuilder> GetAllVariables()
        {
            //JassWeatherAPI apiCaller = new JassWeatherAPI();
            return null;
 
        }

        // GET api/AdminAPI/5
        public JassBuilder GetJassBuilder(int id)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            if (jassbuilder == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return jassbuilder;
        }

        // PUT api/AdminAPI/5
        public HttpResponseMessage PutJassBuilder(int id, JassBuilder jassbuilder)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != jassbuilder.JassBuilderID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(jassbuilder).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/AdminAPI
        public HttpResponseMessage PostJassBuilder(JassBuilder jassbuilder)
        {
            if (ModelState.IsValid)
            {
                db.JassBuilders.Add(jassbuilder);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, jassbuilder);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = jassbuilder.JassBuilderID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/AdminAPI/5
        public HttpResponseMessage DeleteJassBuilder(int id)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            if (jassbuilder == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.JassBuilders.Remove(jassbuilder);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, jassbuilder);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}