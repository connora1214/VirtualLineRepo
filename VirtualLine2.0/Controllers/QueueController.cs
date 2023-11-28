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

        private static int bar = 0;

        private static DateTime startTime = DateTime.MinValue;

        private static Boolean enteringBar = false; //boolean variable that is used for the remove method to change which message the user receives

        public ActionResult Confirmation()
        {
            Establishment e = db.Establishments.Find(bar);
            ViewBag.Message = "You successfully joined the queue for " + e.BarName;
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
           Establishment e = db.Establishments.Find(bar);
           ViewBag.Message = e.BarName;
           return View();
        }

        public ActionResult GetExtendConfirmation()
        {
           return View();
        }

        public ActionResult EnterConfirmation()
        {
           Establishment e = db.Establishments.Find(bar);
           ViewBag.Message = e.BarName;
           return View();
        }
        public ActionResult EnteringBar()
        {
           startTime = DateTime.MinValue; // Stop the timer
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
           Establishment e = db.Establishments.Find(q.Bar);
           ViewBag.Title = e.BarName;
           int estimatedTime = q.Position;
           int hours = estimatedTime / 60;
           int minutes = estimatedTime % 60;
           if(hours == 0)
           {
              if (minutes <= 15)
              {
                 return RedirectToAction("ReadyToEnter", "Queue");
              }
              ViewBag.Message = "Estimated wait time for " + e.BarName + ": " + minutes + "min";             
           }
           else
           {
              ViewBag.Message = "Estimated wait time for " + e.BarName + ": " + hours + "hr " + minutes + "min";
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
              ViewBag.Message = "Estimated wait time for " + db.Establishments.Find(bar).BarName + ": " + LineLength + "min";
           }
           else
           {
              ViewBag.Message = "Estimated wait time for " + db.Establishments.Find(bar).BarName + ": " + hours + "hr " + minutes + "min";
           }
           return View();
        }

        public ActionResult ReadyToEnter()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           Establishment e = db.Establishments.Find(user.Bar);
           ViewBag.Title = e.BarName;
           if(user.Position < 5)
           {             
              return RedirectToAction("startTimer", "Queue");
           }
           ViewBag.Message = user.Username + ", please arrive at " + e.BarName + " within the next " + user.Position + " minutes to gain entry.";
           return View();
        }

        public ActionResult GrantAccess()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           Establishment e = db.Establishments.Find(user.Bar);
           ViewBag.Title = e.BarName;
           return View();
        }
        public ActionResult Timer(string username)
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           Establishment e = db.Establishments.Find(user.Bar);
           ViewBag.Title = e.BarName;
           ViewBag.username = user.Username;
           ViewBag.latitude = e.Latitude;
           ViewBag.longitude = e.Longitude;
           return View();
        }

        public JsonResult CheckTimer(string username)
        {
           //DateTime? startTime = Session["TimerStartTime"] as DateTime?;
           if (startTime == DateTime.MinValue)
           {
            // Handle the case where the timer hasn't been started or session expired
              return Json(new { timeLeft = 0, expired = true }, JsonRequestBehavior.AllowGet);
           }

           TimeSpan elapsed = DateTime.Now - startTime;
           TimeSpan duration = TimeSpan.FromMinutes(15);
           TimeSpan timeLeft = duration - elapsed;

           if (timeLeft.Ticks < 0)
           {
              return Json(new { timeLeft = 0, expired = true }, JsonRequestBehavior.AllowGet);
           }

           return Json(new { timeLeft = timeLeft.TotalSeconds, expired = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserPosition()
        {
           Queue user = db.Queues.Find(User.Identity.Name);
           if (user == null)
           {
              return Json(new { position = -1 }, JsonRequestBehavior.AllowGet);
           }

           bool timerStarted = startTime != DateTime.MinValue;
           return Json(new { position = user.Position }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult startTimer()
        {          
           if (startTime == DateTime.MinValue)
           {
              startTime = DateTime.Now;
           }

           return RedirectToAction("Timer", "Queue");
        }

        public ActionResult ExtendTime()
        {

           startTime = DateTime.Now;

           return RedirectToAction("Timer", "Queue");
        }

        public ActionResult ResetTimerAndGrantAccess()
        {
           // Reset timer 
           startTime = DateTime.Now;
           
           return RedirectToAction("GrantAccess");
        }

        public ActionResult getConfirmation()
        {
           return View();
        }

        public ActionResult GrantAccessConfirmation()
        {
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
        public ActionResult Index(int id)
        {
           //Establishment e = db.Establishments.Find(id);
           bar = id;

           //if user is not logged in redirect them to the account page
           if (User.Identity.Name == "")
           {
              return RedirectToAction("MyAccount", "Home");
           }

           //user is logged in but is not currently in a line
           if (db.Queues.Find(User.Identity.Name) == null)
           {
              //if(bar == "")
              if(bar == 0)
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
               startTime = DateTime.MinValue;
               return RedirectToAction("LeaveConfirmation");
            }
            else 
            {
               startTime = DateTime.MinValue;
               return RedirectToAction("EnterConfirmation");
            }

        }
    }
}