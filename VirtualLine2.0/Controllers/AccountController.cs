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
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace VirtualLine2._0.Controllers
{
    public class AccountEditEntry
    {
       public string Phone { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
    }
   public class ChangePasswordEntry
   {
      public string OldPassword { get; set; }
      public string NewPassword { get; set; }
      public string NewPasswordConfirmation { get; set; }
   }
   public class AccountController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();
      // GET: Account
      public ActionResult Index()
      {
         return View();
      }

      public ActionResult EditConfirmation()
      {
         return View();
      }
      public ActionResult Logout()
      {
         FormsAuthentication.SignOut();
         return View();
      }
      public ActionResult LogoutConfirmation()
      {
         return View();
      }
      public ActionResult ChangePassword()
      {
         return View();
      }

      public ActionResult AccountInfo()
      {
         Account user = db.Accounts.Find(User.Identity.Name);
         ViewBag.Name = user.FirstName + " " + user.LastName;
         ViewBag.Email = user.Email;
         return View();
      }

      public ActionResult EditAccount()
      {
         Account user = db.Accounts.Find(User.Identity.Name);
         ViewBag.FirstName = user.FirstName;
         ViewBag.LastName = user.LastName;
         ViewBag.Phone = user.Phone;
         return View();
      }

      [HttpPost]
      public ActionResult EditAccount(AccountEditEntry entry)
      {
         Account user = db.Accounts.Find(User.Identity.Name);

         //user to prefill the input boxes
         ViewBag.FirstName = user.FirstName;
         ViewBag.LastName = user.LastName;
         ViewBag.Phone = user.Phone;

         //create a temp list of db to remove the current user to check if information matches other users
         List<Account> tempDB = db.Accounts.ToList<Account>();
         tempDB.Remove(user);

         // Check if phone already exists
         if (tempDB.Any(u => u.Phone == entry.Phone))
         {
            ViewBag.Message = "An account with this phone number already exists";
            return View(entry);
         }

         //update user based on input boxes
         user.Phone = entry.Phone;
         user.FirstName = entry.FirstName;
         user.LastName = entry.LastName;
         db.SaveChanges();

         //update queue database if the user is in a line
         Queue lineUser = db.Queues.Find(User.Identity.Name);
         if(lineUser != null)
         {
            lineUser.Phone = user.Phone;
            db.SaveChanges();
         }

         return RedirectToAction("EditConfirmation", "Account");
      }

      [HttpPost]
      public ActionResult ChangePassword(ChangePasswordEntry entry)
      {
         Account user = db.Accounts.Find(User.Identity.Name);
         string hashedPassword = HashPassword(entry.OldPassword);
         if (hashedPassword == user.Password)
         {
            if(entry.NewPassword == entry.NewPasswordConfirmation)
            {
               var hashedNewPassword = HashPassword(entry.NewPassword);
               user.Password = hashedNewPassword;
               db.SaveChanges();
               return RedirectToAction("EditConfirmation", "Account");
            }

            //passwords do not match
            ViewBag.Message = "Passwords do not match";
            return View(entry);
         }

         ViewBag.Message = "Password Incorrect";
         return View(entry);
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