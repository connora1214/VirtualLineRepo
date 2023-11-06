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
   public class AccountController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();
      // GET: Account
      public ActionResult Index()
      {
          return View();
      }

      public ActionResult AddToQueue(QueueEntry entry)
      {

         Account user = new Account();

         

         //queue.Add(entry);
         user.Username = entry.Username;
         user.Phone = entry.Phone;
         db.Accounts.Add(user);
         db.SaveChanges();
         return RedirectToAction("Index");
      }

   }

   
}