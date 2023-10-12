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

        private queueDBEntities3 dbQueue = new queueDBEntities3();

        public ActionResult Index()
        {
            return View(queue);
        }

        [HttpPost]
        public ActionResult AddToQueue(QueueEntry entry)
        {

            Queue user = new Queue();

            //get length of db
            int dblength = 0;
            if (dbQueue.Queues.ToArray()!=null)
            {
                foreach (Queue q in dbQueue.Queues.ToArray())
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
            dbQueue.Queues.Add(user);
            dbQueue.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromQueue(QueueEntry entry)
        {
            Queue user = new Queue();
            String uname = "brian";
            user = dbQueue.Queues.Find(uname);
            
            if (user == null)
            {
                ViewBag.message = "You are not currently in this queue.";
            }
            else
            {
                bool oldValidateOnSaveEnabled = dbQueue.Configuration.ValidateOnSaveEnabled;

                try
                {
                    dbQueue.Configuration.ValidateOnSaveEnabled = false;


                    dbQueue.Queues.Attach(user);
                    dbQueue.Queues.Remove(user);
                    dbQueue.SaveChanges();
                }
                finally
                {
                    dbQueue.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
                }

                foreach (Queue q in dbQueue.Queues.ToArray())
                {
                    if (q.Position > user.Position)
                    {
                        q.Position = q.Position - 1;
                        dbQueue.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Index");
        }
    }
}