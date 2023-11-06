using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.Ajax.Utilities;
using EntityState = System.Data.Entity.EntityState;
using VirtualLine2._0.Controllers;
using VirtualLine2._0.Models;

namespace VirtualLine2._0.Controllers
{
    public class QueueEntry
    {
        public string Username { get; set; }
        public string Phone { get; set; }
        public int Position { get; set; }
    }

    public class QueueController : Controller
    {
        // GET: Queue

        private static List<QueueEntry> queue = new List<QueueEntry>();

        private queueDBEntities3 db = new queueDBEntities3();

        //private string BarName = HomeController.bar;

      private static string bar = "";

      public ActionResult Doggies()
      {
         bar = "Doggies";
         //return the queue index view 
         return RedirectToAction("Index", "Queue");
      }

      public ActionResult Pman()
      {
         bar = "Pman";
         return RedirectToAction("Index", "Queue");
      }

      public ActionResult Champs()
      {
         bar = "Champs";
         return RedirectToAction("Index", "Queue");
      }

      public ActionResult Shandygaff()
      {
         bar = "Shandygaff";
         return RedirectToAction("Index", "Queue");
      }

      public ActionResult Phyrst()
      {
         bar = "Phyrst";
         return RedirectToAction("Index", "Queue");
      }

      public ActionResult Index()
        {
         ViewBag.Title = bar;
         return View(queue);
        }

        [HttpPost]
        public ActionResult AddToQueue(QueueEntry entry)
        {

            Queue user = new Queue();
         
            //get length of db
            int dblength = 0;
            if (db.Queues.ToArray()!=null)
            {
                foreach (Queue q in db.Queues.ToArray())
                {
                    dblength += 1;
                }
            }

            //first on the queue
            if (dblength==0)
            {
                //entry.Position = 1;
                user.Position = 1;
            }
            //not first on the queue
            else
            {
                user.Position = dblength+1;
            }

            //queue.Add(entry);
            user.Username = entry.Username;
            user.Phone = entry.Phone;
            db.Queues.Add(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromQueue(QueueEntry entry)
        {
            Queue user = new Queue();
            String uname = "Connor";
            user = db.Queues.Find(uname);
            
            if (user == null)
            {
                ViewBag.message = "You are not currently in this queue.";
            }
            else
            {
                bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;

                try
                {
                    db.Configuration.ValidateOnSaveEnabled = false;


                    db.Queues.Attach(user);
                    db.Queues.Remove(user);
                    db.SaveChanges();
                }
                finally
                {
                    db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }

                foreach (Queue q in db.Queues.ToArray())
                {
                    if (q.Position > user.Position)
                    {
                        q.Position = q.Position - 1;
                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}