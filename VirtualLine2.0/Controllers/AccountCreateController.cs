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
   public class AccountCreateEntry
   {
      public string Username { get; set; }
      public string phonePart1 { get; set; }
      public string phonePart2 { get; set; }
      public string phonePart3 { get; set; }
      public string Password { get; set; }
      public string ConfirmPassword { get; set; }
      public string Email { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
   } 
   public class AccountCreateController : Controller
   {

      private queueDBEntities3 db = new queueDBEntities3();
      // GET: AccountCreate
      public ActionResult CreateAccount()
      {
         return View();
      }
      public ActionResult Confirmation()
      {
         return View();
      }

      private string HashPassword(string password)
      {
         using (var sha256 = SHA256.Create())
         {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
         }
      }

      [HttpPost]
      public ActionResult CreateAccount(AccountCreateEntry entry)
      {
         Account AccountUser = new Account();

         string phone = entry.phonePart1 + entry.phonePart2 + entry.phonePart3;

         if (entry.Password != entry.ConfirmPassword)
         {
            ViewBag.Message = "The passwords do not match"; 
            return View(entry);
         }

         // Check if username already exists
         if (db.Accounts.Any(u => u.Username == entry.Username))
         {
            ViewBag.Message = "Username already exists";
            return View(entry);
         }
         // Check if email already exists
         if (db.Accounts.Any(u => u.Email == entry.Email))
         {
            ViewBag.Message = "An account with this email already exists";
            return View(entry);
         }
         // Check if phone already exists
         if (db.Accounts.Any(u => u.Phone == phone))
         {
            ViewBag.Message = "An account with this phone number already exists";
            return View(entry);
         }

         var hashedPassword = HashPassword(entry.Password); // Use the HashPassword method as defined earlier

         AccountUser.Username = entry.Username;
         AccountUser.Phone = phone;
         AccountUser.Password = hashedPassword;
         AccountUser.Email = entry.Email;
         AccountUser.FirstName = entry.FirstName;
         AccountUser.LastName = entry.LastName;
         db.Accounts.Add(AccountUser);
         try
         {
            db.SaveChanges();
         }
         catch (DbEntityValidationException ex)
         {
            ViewBag.Message = "Invalid email or phone format";
            return View(entry);
         }
            return RedirectToAction("Confirmation", "AccountCreate");
      }
   }

   
}