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

namespace VirtualLine2._0.Controllers
{
   public class HomeController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();

      public ActionResult Index()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         /*for (int i = 1; i < 20; i++)
         {
            String s = i.ToString();
            Queue user = new Queue();
            user.Position = i;
            user.Username = "u" + s;
            user.Bar = 4;
            user.StartTime = DateTime.MinValue;
            user.Quantity = 1;
            user.timerStarted = false;
            user.enteringBar = false;
            user.ExtendTime = 0;
            db.Queues.Add(user);
            db.SaveChanges();
         }*/

         ViewBag.Locations = db.Establishments.Select(e => e.Location).Distinct().ToList();
         return View();         
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

      public void transferData()
      {
         /*
         List<EnteredUser> tempDB = db.EnteredUsers.ToList<EnteredUser>();
         foreach (EnteredUser e in tempDB)
         {
            VenueEntry v = new VenueEntry();
            v.VenueId = e.VenueId;
            v.VenueName = e.VenueName;
            v.TimeStamp = e.TimeStamp;
            db.VenueEntries.Add(v);
            db.SaveChanges();
            db.EnteredUsers.Remove(e);
            db.SaveChanges();
         }
         */
      }

      public JsonResult GetBarsByLocation(string location)
      {
         var bars = db.Establishments
                     .Where(b => b.Location == location)
                     .Select(b => new
                     {
                        Id = b.Id,
                        BarName = b.BarName,
                        ProfilePicturePath = b.ProfilePicture,
                        BannerPicturePath = b.BannerPicture
                     })
                     .ToList();

         return Json(bars, JsonRequestBehavior.AllowGet);
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