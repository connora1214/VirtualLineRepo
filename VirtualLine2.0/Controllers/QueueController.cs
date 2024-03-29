﻿using System;
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
using NodaTime;
using GeoTimeZone;

namespace VirtualLine2._0.Controllers
{

   public class QueueController : Controller
   {

      private queueDBEntities3 db = new queueDBEntities3();

      private static int bar = 0;

      public ActionResult NotInQueue()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }
      public ActionResult AlreadyInAnotherQueue()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }
      public ActionResult LeaveConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Establishment e = db.Establishments.Find(bar);
         ViewBag.Message = e.BarName;
         return View();
      }

      public ActionResult GetExtendConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Queue user = db.Queues.Find(User.Identity.Name);
         if (user.ExtendTime >= 1)
         {
            ViewBag.Message = "You may only extend your time once.";
         }
         return View();
      }

      public ActionResult QueueNotOpen()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }

      public ActionResult RemovedFromLine()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }

      public ActionResult EnterConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Establishment e = db.Establishments.Find(bar);
         ViewBag.Message = e.BarName;
         return View();
      }
      public ActionResult EnteringBar()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Queue q = db.Queues.Find(User.Identity.Name);
         //q.StartTime = DateTime.MinValue;
         q.enteringBar = true;
         db.SaveChanges();
         return RedirectToAction("RemoveFromQueue", "Queue");
      }
      public ActionResult AlreadyInQueue()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }
      public ActionResult MyQueueActive()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

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
            ViewBag.Message = minutes + " min";
         }
         else
         {
            ViewBag.Message = + hours + " hr " + minutes + " min";
         }
         ViewBag.Venue = e.BarName;
         ViewBag.ProfilePicturePath = e.ProfilePicture;
         ViewBag.BannerPicturePath = e.BannerPicture;
         ViewBag.DefaultProfilePicturePath = "/Content/Images/BrewQueueLogo.jpg";

         return View();
      }
      public ActionResult MyQueueInactive()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

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
            ViewBag.Message = + estimatedTime + " min";
         }
         else
         {
            ViewBag.Message = hours + " hr " + minutes + " min";
         }

         var e = db.Establishments.Find(bar);
         ViewBag.Venue = e.BarName;
         ViewBag.ProfilePicturePath = e.ProfilePicture;
         ViewBag.BannerPicturePath = e.BannerPicture;
         ViewBag.DefaultProfilePicturePath = "/Content/Images/BrewQueueLogo.jpg";
         return View();
      }

      public int calculateWaitTime(int length) 
      {
         Queue q = db.Queues.Find(User.Identity.Name);
         if(q != null)
         {
            bar = q.Bar;
         }

         if (db.EnteredUsers.ToArray().Length > 15) //only use the algorithm if more than 15 people have already entered the bar that night
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
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         Queue user = db.Queues.Find(User.Identity.Name);
         Account account = db.Accounts.Find(User.Identity.Name);
         Establishment e = db.Establishments.Find(user.Bar);
         ViewBag.Title = e.BarName;
         if (user.Position < 6)
         {
            return RedirectToAction("startTimer", "Queue");
         }
         ViewBag.Message = account.FirstName + ", please arrive at " + e.BarName + " within the next " + user.Position + " minutes to gain entry.";
         return View();
      }

      public ActionResult GrantAccess()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         Queue user = db.Queues.Find(User.Identity.Name);
         Establishment e = db.Establishments.Find(user.Bar);
         ViewBag.Title = e.BarName;
         return View();
      }
      public ActionResult Timer(string username)
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         Queue user = db.Queues.Find(User.Identity.Name);
         Establishment e = db.Establishments.Find(user.Bar);
         ViewBag.Title = e.BarName;
         ViewBag.username = user.Username;
         ViewBag.latitude = e.Latitude;
         ViewBag.longitude = e.Longitude;
         return View();
      }

      public JsonResult CheckTimer(bool isFromTimerPage)
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

         /*Establishment e = db.Establishments.Find(User.Identity.Name);
         var tzProvider = DateTimeZoneProviders.Tzdb;

         double lat = Convert.ToDouble(e.Latitude.Value);
         double lon = Convert.ToDouble(e.Longitude.Value);

         // Get the time zone ID using GeoTimeZone
         var tzId = TimeZoneLookup.GetTimeZone(lat, lon).Result;

         // Get the DateTimeZone object from NodaTime using the time zone ID
         var timeZone = tzProvider[tzId];

         // Get the current ZonedDateTime in the establishment's time zone
         var nowInZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

         //var today = nowInZone.DayOfWeek;
         var currentTime = new DateTime(nowInZone.TimeOfDay.Hour, nowInZone.TimeOfDay.Minute, nowInZone.TimeOfDay.Second);*/

         //TimeSpan elapsed = currentTime - user.StartTime;
         TimeSpan elapsed = DateTime.Now - user.StartTime;
         TimeSpan duration = TimeSpan.FromMinutes(15);
         TimeSpan timeLeft = duration - elapsed;

         if (timeLeft.Ticks < 0)
         {
            if (!isFromTimerPage && user.enteringBar == false) //make sure the expired timer message only comes up when the user is not entering the bar
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

            if (db.Queues.Find(user.Username)!=null)
            {
               bar = user.Bar; //make sure the bar variable is correct before removing the user; to be used by the confirmation message 
               db.Queues.Attach(user);
               db.Queues.Remove(user);
               db.SaveChanges();
            }
            
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
               q.Position = q.Position - user.Quantity;
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
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

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
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         //reset timer
         Queue user = db.Queues.Find(User.Identity.Name);

         if (user.ExtendTime >= 1)
         {
            return RedirectToAction("GetExtendConfirmation", "Queue");
         }
         user.StartTime = DateTime.Now;
         user.ExtendTime += 1;
         db.SaveChanges();

         return RedirectToAction("Timer", "Queue");
      }

      public ActionResult ResetTimerAndGrantAccess()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         //Queue user = db.Queues.Find(User.Identity.Name);
         // Reset timer 
         //user.StartTime = DateTime.Now;
         //db.SaveChanges();

         return RedirectToAction("GrantAccess");
      }

      public ActionResult getConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }

      public ActionResult GrantAccessConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
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

      public Boolean checkQueueOpen(int id)
      {
         QueueTime qt = db.QueueTimes.Find(id);

         Establishment e = db.Establishments.Find(id);

         if (qt != null)
         {
            // Use NodaTime's DateTimeZoneProviders.Tzdb to get the IDateTimeZoneProvider
            var tzProvider = DateTimeZoneProviders.Tzdb;

            double lat = Convert.ToDouble(e.Latitude.Value);
            double lon = Convert.ToDouble(e.Longitude.Value);

            // Get the time zone ID using GeoTimeZone
            var tzId = TimeZoneLookup.GetTimeZone(lat, lon).Result;

            // Get the DateTimeZone object from NodaTime using the time zone ID
            var timeZone = tzProvider[tzId];

            // Get the current ZonedDateTime in the establishment's time zone
            var nowInZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

            var today = nowInZone.DayOfWeek;
            var currentTime = new TimeSpan(nowInZone.TimeOfDay.Hour, nowInZone.TimeOfDay.Minute, nowInZone.TimeOfDay.Second);

            //var today = DateTime.Now.DayOfWeek;
            //TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (today == IsoDayOfWeek.Sunday)
            {
               // Handle the case where the current time is early in the day but before the previous night's closing time
               if (qt.SaturdayClose < qt.SaturdayOpen)
               {
                  if (currentTime < qt.SundayClose)
                  {
                     return true;
                  }
               }

               // Check if closing time is on the next day
               if (qt.SundayClose < qt.SundayOpen)
               {
                  // It's still "open hours" if the current time is after the opening time or before midnight.
                  // Additionally, if the current time is after midnight but before the closing time, it's also open.
                  if (currentTime >= qt.SundayOpen || currentTime < qt.SundayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  // Normal scenario where closing time is after opening time on the same day
                  if (currentTime >= qt.SundayOpen && currentTime <= qt.SundayClose)
                  {
                     return true;
                  }
               }
            }
            else if (today == IsoDayOfWeek.Monday)
            {
               if (qt.SundayClose < qt.SundayOpen)
               {
                  if (currentTime < qt.SundayClose)
                  {
                     return true;
                  }
               }

               if (qt.MondayClose < qt.MondayOpen)
               {
                  if (currentTime >= qt.MondayOpen || currentTime < qt.MondayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.MondayOpen && currentTime <= qt.MondayClose)
                  {
                     return true;
                  }
               }
            }
            else if (today == IsoDayOfWeek.Tuesday)
            {
               if (qt.MondayClose < qt.MondayOpen)
               {
                  if (currentTime < qt.MondayClose)
                  {
                     return true;
                  }
               }

               if (qt.TuesdayClose < qt.TuesdayOpen)
               {
                  if (currentTime >= qt.TuesdayOpen || currentTime < qt.TuesdayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.TuesdayOpen && currentTime <= qt.TuesdayClose)
                  {
                     return true;
                  }
               }
            }
            else if (today == IsoDayOfWeek.Wednesday)
            {
               if (qt.TuesdayClose < qt.TuesdayOpen)
               {
                  if (currentTime < qt.TuesdayClose)
                  {
                     return true;
                  }
               }

               if (qt.WednesdayClose < qt.WednesdayOpen)
               {
                  if (currentTime >= qt.WednesdayOpen || currentTime < qt.WednesdayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.WednesdayOpen && currentTime <= qt.WednesdayClose)
                  {
                     return true;
                  }
               }
            }
            else if (today == IsoDayOfWeek.Thursday)
            {
               if (qt.WednesdayClose < qt.WednesdayOpen)
               {
                  if (currentTime < qt.WednesdayClose)
                  {
                     return true;
                  }
               }

               if (qt.ThursdayClose < qt.ThursdayOpen)
               {
                  if (currentTime >= qt.ThursdayOpen || currentTime < qt.ThursdayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.ThursdayOpen && currentTime <= qt.ThursdayClose)
                  {
                     return true;
                  }
               }
            }
            else if (today == IsoDayOfWeek.Friday)
            {
               if (qt.ThursdayClose < qt.ThursdayOpen)
               {
                  if (currentTime < qt.ThursdayClose)
                  {
                     return true;
                  }                 
               }

               if (qt.FridayClose < qt.FridayOpen)
               {
                  if (currentTime >= qt.FridayOpen || currentTime < qt.FridayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.FridayOpen || currentTime < qt.FridayClose)
                  {
                     return true;
                  }
               }
            }
            else //today is saturday
            {
               if (qt.FridayClose < qt.FridayOpen)
               {
                  if (currentTime < qt.FridayClose)
                  {
                     return true;
                  }
               }

               if (qt.SaturdayClose < qt.SaturdayOpen)
               {
                  if (currentTime >= qt.SaturdayOpen || currentTime < qt.SaturdayClose)
                  {
                     return true;
                  }
               }
               else
               {
                  if (currentTime >= qt.SaturdayOpen && currentTime <= qt.SaturdayClose)
                  {
                     return true;
                  }
               }
            }
         }

         return false;
      }

      public ActionResult Index(int id)
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         bar = id;

         if (checkQueueOpen(id) == false)
         {
            return RedirectToAction("QueueNotOpen", "Queue");
         }


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
      public ActionResult AddToQueue(string numberSelect)
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         Queue user = new Queue();

         //get length of db
         int LineLength = 0;
         //int lastUserPos = 0;

         //select only the queue entries of the bar that the user is joining the queue of
         var BarQueue = from q in db.Queues select q;
         BarQueue = BarQueue.Where(q => q.Bar.Equals(bar));

         if (db.Queues.ToArray() != null)
         {
            foreach (Queue q in BarQueue.ToArray())
            {
               LineLength += q.Quantity;
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
            int lastPos = db.Queues.Max(q => q.Position);
            Queue lastUser = db.Queues.FirstOrDefault(q => q.Position.Equals(lastPos));
            
            user.Position = lastUser.Position + lastUser.Quantity;
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
            user.Quantity = Int32.Parse(numberSelect);
            user.timerStarted = false;
            user.enteringBar = false;
            user.ExtendTime = 0;


            if (db.Queues.Find(user.Username) != null)
            {
               return RedirectToAction("AlreadyInQueue");
            }

            if (user.Quantity == 1)
            {
               RedirectToAction("Pay5", "Queue");
            }

            db.Queues.Add(user);
            db.SaveChanges();
            return RedirectToAction("MyQueue", "Queue");
         }
         //if user is not logged in direct them to the login page
         else
         {
            return RedirectToAction("MyAccount", "Home");
         }
      }

      //[Route("https://pay.brew-queue.com/queuepass5")]
      //public ActionResult

      public ActionResult RemoveFromQueue()
      {
         if (User.Identity.Name == "")
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
            bool? enteringBar = user.enteringBar;

            if (enteringBar == true)
            {
               int num = 0;
               while(num < user.Quantity)
               {
                  Establishment e = db.Establishments.Find(user.Bar);

                  var tzProvider = DateTimeZoneProviders.Tzdb;                  
                  double lat = Convert.ToDouble(e.Latitude.Value);
                  double lon = Convert.ToDouble(e.Longitude.Value);

                  // Get the time zone ID using GeoTimeZone
                  var tzId = TimeZoneLookup.GetTimeZone(lat, lon).Result;

                  // Get the DateTimeZone object from NodaTime using the time zone ID
                  var timeZone = tzProvider[tzId];

                  // Get the current ZonedDateTime in the establishment's time zone
                  var nowInZone = SystemClock.Instance.GetCurrentInstant().InZone(timeZone);

                  var currentDate = nowInZone.Date;
                  var currentTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, nowInZone.TimeOfDay.Hour, nowInZone.TimeOfDay.Minute, nowInZone.TimeOfDay.Second);
                  
                  EnteredUser u = new EnteredUser();
                  u.VenueName = db.Establishments.Find(user.Bar).BarName;
                  u.TimeStamp = currentTime;
                  u.VenueId = user.Bar;
                  db.EnteredUsers.Add(u);
                  num += 1;
               }             
               db.SaveChanges();
            }
            RemoveUserFromQueue(user);

            if (enteringBar == false)
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
}