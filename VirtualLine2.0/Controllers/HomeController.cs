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
using System.Web.Security;
using OneSignalApi.Api;
using OneSignalApi.Client;
using OneSignalApi.Model;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VirtualLine2._0.Controllers
{
   public class HomeController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();

      private static bool guest = false;

      public ActionResult SetGuest()
      {
         guest = true;
         return RedirectToAction("Index");
      }
      public ActionResult Index()
      {
         /*foreach ( Account a in db.Accounts.ToList())
         {
            a.BrewQueueCredit = (decimal?)0.0;
            db.SaveChanges();
         }*/

        /* Account a = db.Accounts.Find("connora1214");
         a.BrewQueueCredit = (decimal?)10.0;
         db.SaveChanges();*/

         if (User.Identity.Name == "")
         {
            if (!guest)
            {
               return RedirectToAction("MyAccount", "Home");
            }
            guest = false;
         }

         //List<string> names = new List<string> { "BeccaLebrao", "nerimias11", "jrgavlik@gmail.com", "test", "admin", "connora1214" };
         /*for (int i = 1; i < 26; i++)
         {
            String s = i.ToString();
            *//*Account a = new Account();
            a.Username = "u" + s;
            a.FirstName = "First_Name";
            a.LastName = "Last_Name";
            a.Password = HashPassword("50Butterfly!");
            a.Email = "u" + s + "@gmail.com";
            Random rand = new Random();
            int randomNumber1 = rand.Next(10000, 99999);
            int randomNumber2 = rand.Next(10000, 99999);
            a.Phone = randomNumber1.ToString() + randomNumber2.ToString();
            db.Accounts.Add(a);
            db.SaveChanges();*//*

            Queue user = new Queue();
            user.Position = i;
            user.Username = "u" + s;
            //user.Username = names[i - 1];
            user.Bar = 4;
            user.StartTime = DateTime.MinValue;
            user.Quantity = 1;
            user.timerStarted = false;
            user.enteringBar = false;
            user.PricePoint = (decimal?)5.0;
            user.ExtendTime = 0;
            db.Queues.Add(user);
            db.SaveChanges();
         }*/

         /*VenueEntry v = new VenueEntry();
         v.Username = "connora1214";
         v.TimeStamp = DateTime.Now;
         v.VenueId = 4;
         v.VenueName = "Doggies";
         v.PricePoint = (decimal?)5.0;
         db.VenueEntries.Add(v);
         db.SaveChanges();*/

         /*if (initialOpen)
         {
            initialOpen = false;
            return RedirectToAction("MyAccount", "Home");
         }*/
         //List<string> ExpiringUsers = checkExpiringTimers();



         ViewBag.Locations = db.Establishments.Select(e => e.Location).Distinct().ToList();
         return View();         
      }

      private List<string> checkExpiringTimers()
      {
         List<string> Ids = new List<string>();

         // Retrieve all queue objects where timerStarted is true
         var queuesExpiring = db.Queues.Where(q => q.timerStarted);

         // Check if there are any queue objects
         if (queuesExpiring.Any())
         {
            foreach (Queue q in queuesExpiring.ToArray())
            {
               Account a = db.Accounts.Find(q.Username);
               // Calculate the elapsed time since timerStarted
               TimeSpan elapsed = DateTime.UtcNow - q.StartTime;

               // Check if 15 minutes have elapsed
               if (elapsed.TotalMinutes >= 15)
               {
                  if (a.OneSignalPlayerId != null)
                  {
                     if (!Ids.Contains(a.OneSignalPlayerId))
                     {
                        Ids.Add(a.OneSignalPlayerId);
                     }
                  }
                  RemoveUserFromQueue(q);
               }
            }

            db.SaveChanges();
         }
         return Ids;
      }

      private void RemoveUserFromQueue(Queue user)
      {
         bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;

         try
         {
            db.Configuration.ValidateOnSaveEnabled = false;

            if (db.Queues.Find(user.Username) != null)
            {
               db.Queues.Attach(user);
               db.Queues.Remove(user);
               db.SaveChanges();
            }

         }
         finally
         {
            db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
         }

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

      private string HashPassword(string password)
      {
         using (var sha256 = SHA256.Create())
         {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
         }
      }

      public JsonResult FindNearestLocation(double userLatitude, double userLongitude)
      {
         var establishments = db.Establishments.ToList(); // Or a more selective query

         Establishment nearestEstablishment = null;
         double minDistance = double.MaxValue;

         foreach (var establishment in establishments)
         {
            if (establishment.Latitude.HasValue && establishment.Longitude.HasValue)
            {
               double establishmentLatitude = Convert.ToDouble(establishment.Latitude.Value);
               double establishmentLongitude = Convert.ToDouble(establishment.Longitude.Value);

               double distance = CalculateDistance(userLatitude, userLongitude, establishmentLatitude, establishmentLongitude);
               if (distance < minDistance)
               {
                  minDistance = distance;
                  nearestEstablishment = establishment;
               }
            }
         }

         return Json(new { nearestLocation = nearestEstablishment?.Location }, JsonRequestBehavior.AllowGet);
      }

      public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
      {
         const double R = 6371; // Radius of the Earth in kilometers
         double latRadians1 = DegreesToRadians(lat1);
         double latRadians2 = DegreesToRadians(lat2);
         double latRadiansDelta = DegreesToRadians(lat2 - lat1);
         double lonRadiansDelta = DegreesToRadians(lon2 - lon1);

         double a = Math.Sin(latRadiansDelta / 2) * Math.Sin(latRadiansDelta / 2) +
                    Math.Cos(latRadians1) * Math.Cos(latRadians2) *
                    Math.Sin(lonRadiansDelta / 2) * Math.Sin(lonRadiansDelta / 2);
         double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
         return R * c; // Distance in kilometers
      }

      private double DegreesToRadians(double degrees)
      {
         return degrees * (Math.PI / 180);
      }

      public JsonResult GetBarsByLocation(string location)
      {
         var bars = db.Establishments.Where(b => b.Location == location).ToList();

         List<object> barDetails = new List<object>();

         foreach (var bar in bars)
         {
            
            int time = calculateWaitTime(bar.Id);  // Assuming this method now takes a bar's ID and calculates wait time based on that.
            int hours = time / 60;
            int minutes = time % 60;

            String waitTime = "";

            if (hours == 0)
            {
               waitTime = minutes + " min";
            }
            else
            {
               if (minutes == 0)
               {
                  waitTime = + hours + " h";
               }
               waitTime = + hours + " h " + minutes + " m";
            }
            barDetails.Add(new
            {
               Id = bar.Id,
               BarName = bar.BarName,
               ProfilePicturePath = bar.ProfilePicture,
               BannerPicturePath = bar.BannerPicture,
               WaitTime = waitTime  // Include calculated wait time
            });
         }

         return Json(barDetails, JsonRequestBehavior.AllowGet);

         //return Json(bars, JsonRequestBehavior.AllowGet);
      }

      public int calculateWaitTime(int id)
      {
         Establishment bar = db.Establishments.Find(id);

         var BarQueue = from q in db.Queues select q;
         BarQueue = BarQueue.Where(q => q.Bar.Equals(bar.Id));

         int LineLength = 0;

         if (BarQueue.ToArray() != null)
         {
            LineLength = BarQueue.ToArray().Sum(u => u.Quantity);
         }

         if (db.EnteredUsers.Where(e => e.VenueId.Equals(bar.Id)).ToArray().Length > 15) //only use the algorithm if more than 15 people have already entered the bar that night
         {
            int numRecentEntriesSixty = 0;
            int numRecentEntriesThirty = 0;
            int numRecentEntriesFifteen = 0;

            var sixtyMinutesAgo = DateTime.Now.AddMinutes(-60);

            var recentEntriesSixty = db.EnteredUsers.Where(e => e.VenueId.Equals(bar.Id) && e.TimeStamp > sixtyMinutesAgo).ToList();

            if (recentEntriesSixty.Count > 0)
            {
               //if there are enteries from 1 hour ago, check entries from 30 minutes to get a more specific wait time
               var thirtyMinutesAgo = DateTime.Now.AddMinutes(-30);
               var recentEntriesThirty = db.EnteredUsers.Where(e => e.VenueId.Equals(bar.Id) && e.TimeStamp > thirtyMinutesAgo).ToList();

               if (recentEntriesThirty.Count > 0)
               {
                  //if there are enteries from 30 minutes ago, check entries from 15 minutes to get a more specific wait time
                  numRecentEntriesThirty = recentEntriesThirty.Count;

                  var FifteenMinutesAgo = DateTime.Now.AddMinutes(-15);
                  var recentEntriesFifteen = db.EnteredUsers.Where(e => e.VenueId.Equals(bar.Id) && e.TimeStamp > FifteenMinutesAgo).ToList();

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
               int estimatedTime = (int)(LineLength / enterPerMinuteWeighted);

               return estimatedTime;
            }
         }

         //probably will only enter this if statement if no one from the queue has entered yet for the night
         if (LineLength != 0)
         {
            return LineLength;
         }
         return 0;
      }

      public ActionResult MyAccount()
      {
         //user is logged in already
         if(User.Identity.Name != "")
         {
            ViewBag.Message = "My Account";
            return RedirectToAction("AccountInfo", "Account");
         }
         
         return View();
      }

      public ActionResult About()
      {
         return View();
      }

   }
}