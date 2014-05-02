using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using System.Threading.Tasks;
using System.Web.ApplicationServices;

namespace JassWeather.Controllers
{
    public class TaskController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Task/

        public ActionResult Index()
        {
            return View(db.JassTasks.ToList());
        }

        public ActionResult QueryTaskStatus()
        {
            string result = "QueryBuilderStatus";
            if (JassController.task == null)
            {
                result = "No Active Tasks Found";
            }
            else
            {
                result = JassController.task.Status.ToString();
            }

            ViewBag.Message = result;
            return View();
        }

        //
        // GET: /Task/Details/5

        public ActionResult Details(int id = 0)
        {
            JassTask jasstask = db.JassTasks.Find(id);
            if (jasstask == null)
            {
                return HttpNotFound();
            }
            return View(jasstask);
        }

        //
        // GET: /Task/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Task/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassTask jasstask)
        {
            if (ModelState.IsValid)
            {
                db.JassTasks.Add(jasstask);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasstask);
        }

        //
        // GET: /Task/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassTask jasstask = db.JassTasks.Find(id);
            if (jasstask == null)
            {
                return HttpNotFound();
            }
            return View(jasstask);
        }

        //
        // POST: /Task/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassTask jasstask)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasstask).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasstask);
        }

        //
        // GET: /Task/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassTask jasstask = db.JassTasks.Find(id);
            if (jasstask == null)
            {
                return HttpNotFound();
            }
            return View(jasstask);
        }

        //
        // POST: /Task/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassTask jasstask = db.JassTasks.Find(id);
            db.JassTasks.Remove(jasstask);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}