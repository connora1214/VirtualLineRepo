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

      public ActionResult Index()
      {
         return View();
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