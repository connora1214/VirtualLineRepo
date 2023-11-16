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
   /*
    public class QueueEntry
    {
        public string Username { get; set; }
        public string Phone { get; set; }
        public int Position { get; set; }
    }
   */

    public class QueueController : Controller
    {
        //private static List<QueueEntry> queue = new List<QueueEntry>();

      private queueDBEntities3 db = new queueDBEntities3();

      private static string bar = "";

      public ActionResult Doggies()
      {
         bar = "Doggies";
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

        public ActionResult Confirmation()
        {
            ViewBag.Message = "You successfully joined the queue for " + bar;
            return View();
        }
        
        public ActionResult NotInQueue()
        {
            return View();
        }
        public ActionResult AlreadyInAnotherQueue()
        {
           return View();
        }
        public ActionResult LeaveConfirmation()
        {
           return View();
        }
        public ActionResult AlreadyInQueue()
        {
           return View();
        }
        public ActionResult MyQueueActive()
        {
           Queue q = db.Queues.Find(User.Identity.Name);
           ViewBag.Title = q.Bar;
           int estimatedTime = q.Position;
           int hours = estimatedTime / 60;
           int minutes = estimatedTime % 60;
           if(hours == 0)
           {
              ViewBag.Message = "Estimated wait time for " + q.Bar + ": " + minutes + "min";             
           }
           else
           {
            ViewBag.Message = "Estimated wait time for " + q.Bar + ": " + hours + "hr " + minutes + "min";
           }
           
           return View();
        }
        public ActionResult MyQueueInactive()
        {
           var BarQueue = from q in db.Queues select q;
           BarQueue = BarQueue.Where(q => q.Bar.Equals(bar));
           int LineLength = 0;

           if (db.Queues.ToArray() != null)
           {
              foreach (Queue q in BarQueue.ToArray())
              {
                 LineLength += 1;
              }
           }

           int hours = LineLength / 60;
           int minutes = LineLength % 60;
           if (hours == 0)
           {
              ViewBag.Message = "Estimated wait time for " + bar + ": " + LineLength + "min";
           }
           else
           {
              ViewBag.Message = "Estimated wait time for " + bar + ": " + hours + "hr " + minutes + "min";
           }
           return View();
        }
        public ActionResult Index()
        {
           //if user is not logged in redirect them to the account page
           if (User.Identity.Name == "")
           {
              return RedirectToAction("MyAccount", "Home");
           }

           //user is logged in but is not currently in a line
           if (db.Queues.Find(User.Identity.Name) == null)
           {
              if(bar == "")
              {
               return RedirectToAction("NotinQueue", "Queue");
              }
              return RedirectToAction("MyQueueInactive", "Queue");
           }
           //user is logged in and is currently in a line
           else
           {
              Queue user = db.Queues.Find(User.Identity.Name);
              if (user.Bar == bar)
              {
                 return RedirectToAction("MyQueueActive", "Queue");
              }
              else 
              {
                 return RedirectToAction("AlreadyInAnotherQueue", "Queue");
              }
           }          
        }

        [HttpPost]
        public ActionResult AddToQueue()
        {

            Queue user = new Queue();
         
            //get length of db
            int LineLength = 0;

            //select only the queue entries of the bar that the user is joining the queue of
            var BarQueue = from q in db.Queues select q;
            BarQueue = BarQueue.Where(q => q.Bar.Equals(bar));
            

            if (db.Queues.ToArray()!=null)
            {
                foreach (Queue q in BarQueue.ToArray())
                {
                    LineLength += 1;
                }
            }

            //first on the queue
            if (LineLength==0)
            {
                user.Position = 1;
            }
            //not first on the queue
            else
            {
                user.Position = LineLength + 1;
            }

            //user is logged in
            if(User.Identity.Name != "")
            {
               //get the account info of the logged in user
               Account account = db.Accounts.Find(User.Identity.Name);
               user.Username = account.Username;               
               user.Phone = account.Phone;
               user.Bar = bar;

               if (db.Queues.Find(user.Username) != null)
               {
                  return RedirectToAction("AlreadyInQueue");
               }

               db.Queues.Add(user);
               db.SaveChanges();
               return RedirectToAction("Confirmation", "Queue");
            }
            //if user is not logged in direct them to the login page
            else
            {
               return RedirectToAction("MyAccount", "Home");
            }
            
        }

        public ActionResult RemoveFromQueue()
        {
            //if user is not logged in, send them to account page
            if(User.Identity.Name == "")
            {
               return RedirectToAction("MyAccount", "Home");
            }

            Queue user = db.Queues.Find(User.Identity.Name);
        
            if (user == null)
            {
               ViewBag.Message = "You are not currently in line.";
               return RedirectToAction("NotInQueue");              
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

                bar = user.Bar;
                var BarQueue = from q in db.Queues select q;
                BarQueue = BarQueue.Where(q => q.Bar.Equals(user.Bar));
                foreach (Queue q in BarQueue.ToArray())
                {
                    if (q.Position > user.Position)
                    {
                        q.Position = q.Position - 1;
                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("LeaveConfirmation");
        }
    }
}