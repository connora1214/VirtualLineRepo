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

    public class QueueController : Controller
    {

      private queueDBEntities3 db = new queueDBEntities3();

      private static string bar = "";

      private static Boolean enteringBar = false; //boolean variable that is used for the remove method to change which message the user receives

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
           Queue user = db.Queues.Find(User.Identity.Name);
           ViewBag.Message = bar;
           return View();
        }
        public ActionResult EnterConfirmation()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           ViewBag.Message = bar;
           return View();
        }
        public ActionResult EnteringBar()
        {
           enteringBar = true;
           return RedirectToAction("RemoveFromQueue", "Queue");
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
              if (minutes <= 15)
              {
                 return RedirectToAction("ReadyToEnter", "Queue");
              }
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

        public ActionResult ReadyToEnter()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           ViewBag.Title = user.Bar;
           ViewBag.Message = user.Username + ", please arrive at " + user.Bar + " within the next " + user.Position + " minutes to gain entry.";
           return View();
        }
        public ActionResult getConfirmation()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           ViewBag.Title = user.Bar;
           return View();
        }
         public ActionResult MyQueue()   // user clicks on the MyQueue tab
        {
           //if user is not logged in redirect them to the account page
           if (User.Identity.Name == "")
           {
              return RedirectToAction("MyAccount", "Home");
           }
           //user is logged in but is not currently in a line
           if (db.Queues.Find(User.Identity.Name) == null)
           {
              return RedirectToAction("NotInQueue", "Queue");
           }

           return RedirectToAction("MyQueueActive", "Queue");
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
                    bar = user.Bar; //make sure the bar variable is correct before removing the user; to be used by the confirmation message 
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

            if(enteringBar == false)
            {
               return RedirectToAction("LeaveConfirmation");
            }
            else 
            {
               return RedirectToAction("EnterConfirmation");
            }
        }
    }
}