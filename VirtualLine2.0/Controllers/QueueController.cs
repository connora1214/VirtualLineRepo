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

      //private static DateTime startTime = DateTime.MinValue;

      //private static Boolean enteringBar = false; //boolean variable that is used for the remove method to change which message the user receives

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
         Queue q = db.Queues.Find(User.Identity.Name);
         q.StartTime = DateTime.MinValue;
         q.enteringBar = true;
         db.SaveChanges();
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

         int estimatedTime = calculateWaitTime(q.Position);
         int hours = estimatedTime / 60;
         int minutes = estimatedTime % 60;

         if (hours == 0)
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

         int estimatedTime = calculateWaitTime(LineLength);
         int hours = estimatedTime / 60;
         int minutes = estimatedTime % 60;

         if (hours == 0)
         {
            ViewBag.Message = "Estimated wait time for " + db.Establishments.Find(bar).BarName + ": " + estimatedTime + "min";
         }
         else
         {
            ViewBag.Message = "Estimated wait time for " + db.Establishments.Find(bar).BarName + ": " + hours + "hr " + minutes + "min";
         }
         return View();
      }

      public int calculateWaitTime(int length) 
      {
         Queue q = db.Queues.Find(User.Identity.Name);
         if(q != null)
         {
            bar = q.Bar;
         }

         if (db.EnteredUsers.ToArray().Length > 0)
         {
            int numRecentEntriesSixty = 0;
            int numRecentEntriesThirty = 0;
            int numRecentEntriesFifteen = 0;

            var sixtyMinutesAgo = DateTime.Now.AddMinutes(-60);

            var recentEntriesSixty = db.EnteredUsers.Where(e => e.VenueId.Equals(bar) && e.TimeStamp > sixtyMinutesAgo).ToList();

            if (recentEntriesSixty.Count > 0)
            {
               //if there are enteries from 1 hour ago, check entries from 30 minutes to get a more specific wait time
               var thirtyMinutesAgo = DateTime.Now.AddMinutes(-30);
               var recentEntriesThirty = db.EnteredUsers.Where(e => e.VenueId.Equals(bar) && e.TimeStamp > thirtyMinutesAgo).ToList();

               if (recentEntriesThirty.Count > 0) 
               {
                  //if there are enteries from 30 minutes ago, check entries from 15 minutes to get a more specific wait time
                  numRecentEntriesThirty = recentEntriesThirty.Count;

                  var FifteenMinutesAgo = DateTime.Now.AddMinutes(-15);
                  var recentEntriesFifteen = db.EnteredUsers.Where(e => e.VenueId.Equals(bar) && e.TimeStamp > FifteenMinutesAgo).ToList();

                  if (recentEntriesFifteen.Count > 0)
                  {
                     numRecentEntriesFifteen = recentEntriesFifteen.Count;
                  }
               }

               numRecentEntriesSixty = recentEntriesSixty.Count;
              
               double enterPerMinuteSixty = numRecentEntriesSixty / 60.0;
               double enterPerMinuteThirty = numRecentEntriesThirty / 30.0;
               double enterPerMinuteFifteen = numRecentEntriesFifteen / 15.0;

               double enterPerMinuteWeighted = (.50 * enterPerMinuteThirty) + (.35 * enterPerMinuteSixty) + (.15 * enterPerMinuteFifteen);
               int estimatedTime = (int)(length / enterPerMinuteWeighted);

               return estimatedTime;
            }
            

            
         }

         //probably will only enter this if statement if no one from the queue has entered yet for the night
         if (length != 0)
         {
            return length;
         }
         return 0;
      }

      public ActionResult ReadyToEnter()
      {
         Queue user = db.Queues.Find(User.Identity.Name);
         Establishment e = db.Establishments.Find(user.Bar);
         ViewBag.Title = e.BarName;
         if (user.Position < 6)
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

      public JsonResult CheckTimer(bool isFromTimerPage = false)
      {
         if (!User.Identity.IsAuthenticated)
         {
            // User is not logged in
            return Json(new { isAuthenticated = false }, JsonRequestBehavior.AllowGet);
         }

         Queue user = db.Queues.Find(User.Identity.Name);

         if (user == null || !user.timerStarted)
         {
            // User is not in the queue or timer isn't started
            return Json(new { isAuthenticated = true, isInQueue = false }, JsonRequestBehavior.AllowGet);
         }

         TimeSpan elapsed = DateTime.Now - user.StartTime;
         TimeSpan duration = TimeSpan.FromMinutes(120);
         TimeSpan timeLeft = duration - elapsed;

         if (timeLeft.Ticks < 0)
         {
            if (!isFromTimerPage)
            {
               RemoveUserFromQueue(user);
            }
            return Json(new { isAuthenticated = true, isInQueue = true, timeLeft = 0, expired = true }, JsonRequestBehavior.AllowGet);
         }

         return Json(new { isAuthenticated = true, isInQueue = true, timeLeft = timeLeft.TotalSeconds, expired = false }, JsonRequestBehavior.AllowGet);
      }

      private void RemoveUserFromQueue(Queue user)
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

      public JsonResult GetUserPosition()
      {
         Queue user = db.Queues.Find(User.Identity.Name);
         if (user == null)
         {
            return Json(new { position = -1 }, JsonRequestBehavior.AllowGet);
         }

         return Json(new { position = user.Position, timerStarted = user.timerStarted }, JsonRequestBehavior.AllowGet);
      }

      public ActionResult startTimer()
      {
         Queue user = db.Queues.Find(User.Identity.Name);

         if (user.timerStarted == false)
         {
            user.timerStarted = true;
            user.StartTime = DateTime.Now;
            db.SaveChanges();
         }

         return RedirectToAction("Timer", "Queue");
      }

      public ActionResult ExtendTime()
      {
         //reset timer
         Queue user = db.Queues.Find(User.Identity.Name);
         user.StartTime = DateTime.Now;
         db.SaveChanges();

         return RedirectToAction("Timer", "Queue");
      }

      public ActionResult ResetTimerAndGrantAccess()
      {
         Queue user = db.Queues.Find(User.Identity.Name);
         // Reset timer 
         user.StartTime = DateTime.Now;
         db.SaveChanges();

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
         bar = id;

         //if user is not logged in redirect them to the account page
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         //user is logged in but is not currently in a line
         if (db.Queues.Find(User.Identity.Name) == null)
         {
            if (bar == 0)
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


         if (db.Queues.ToArray() != null)
         {
            foreach (Queue q in BarQueue.ToArray())
            {
               LineLength += 1;
            }
         }

         //first on the queue
         if (LineLength == 0)
         {
            user.Position = 1;
         }
         //not first on the queue
         else
         {
            user.Position = LineLength + 1;
         }

         //user is logged in
         if (User.Identity.Name != "")
         {
            //get the account info of the logged in user
            Account account = db.Accounts.Find(User.Identity.Name);
            user.Username = account.Username;
            user.Phone = account.Phone;
            user.Bar = bar;
            user.StartTime = DateTime.MinValue;
            user.timerStarted = false;
            user.enteringBar = false;

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
            bool? enteringBar = user.enteringBar;

            if (enteringBar == true)
            {
               EnteredUser u = new EnteredUser();
               u.VenueName = db.Establishments.Find(user.Bar).BarName;
               u.TimeStamp = DateTime.Now;
               u.VenueId = user.Bar;
               db.EnteredUsers.Add(u);
               db.SaveChanges();
            }
            RemoveUserFromQueue(user);

            if (enteringBar == false)
            {
               //startTime = DateTime.MinValue;
               return RedirectToAction("LeaveConfirmation");
            }
            else
            {
               //startTime = DateTime.MinValue;
               return RedirectToAction("EnterConfirmation");
            }
         }
      }
   }
}