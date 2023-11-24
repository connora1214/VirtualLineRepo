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
   public class HomeController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();

      public ActionResult Index()
      {

         ViewBag.Locations = db.Establishments.Select(e => e.Location).Distinct().ToList();
         return View();
      }

      public JsonResult GetBarsByLocation(string location)
      {
         var barNames = db.Establishments.Where(e => e.Location == location).Select(e => new { e.Id, e.BarName }).ToList();

         return Json(barNames, JsonRequestBehavior.AllowGet);
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

         public ActionResult AboutUs()
      {
         return View();
      }

   }
}