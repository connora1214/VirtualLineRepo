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
using System.Security.Cryptography;
using System.Text;

namespace VirtualLine2._0.Controllers
{
   public class AccountLoginEntry
   {
      public string Username { get; set; }
      public string Password { get; set; }
   }

   public class AccountLoginController : Controller
   {

      private queueDBEntities3 db = new queueDBEntities3();

      public ActionResult AccountLogin()
      {
          return View();
      }

      public ActionResult Confirmation()
      {
         Account user = db.Accounts.Find(User.Identity.Name);
         ViewBag.Message = "Hello, " + user.FirstName;
         return View();
      }

      public ActionResult ForgotPassword(AccountLoginEntry entry)
      {
         return View();
      }

         [HttpPost]
      public ActionResult AccountLogin(AccountLoginEntry entry)
      {

         using (var context = new queueDBEntities3())  
         {
               // hash the password
            string hashedPassword = HashPassword(entry.Password);  

               // Check if the user exists with the provided username and hashed password
            var user = context.Accounts.FirstOrDefault(u => u.Username == entry.Username && u.Password == hashedPassword);

            if (user != null)
            {
               FormsAuthentication.SetAuthCookie(entry.Username, false);
               return RedirectToAction("Confirmation", "AccountLogin");
            }
            else
            {
               ViewBag.Message = "Invalid username or password";
               return View();
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
   }

   
}