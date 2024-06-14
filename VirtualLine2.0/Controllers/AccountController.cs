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
using System.IO;

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

   public class DeleteAccountEntry
   {
      public string Username { get; set; }
      public string Password { get; set; }
   }

   public class AccountController : Controller
   {
      private queueDBEntities3 db = new queueDBEntities3();
      // GET: Account
      public ActionResult Index()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         return View();
      }

      public ActionResult Privacy()
      {
         return View();
      }

      public ActionResult History()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }


         var historyEntries = db.VenueEntries.Where(v => v.Username == User.Identity.Name).ToList();

         if (historyEntries.Any())
         {
            ViewBag.Data = "hasData";
         }
         else
         {
            ViewBag.Data = "";
         }

         return View(historyEntries.OrderByDescending(v => v.TimeStamp));
      }

      public ActionResult EditConfirmation()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

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
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }

      public ActionResult AccountInfo()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Account user = db.Accounts.Find(User.Identity.Name);
         ViewBag.Name = user.FirstName + " " + user.LastName;
         ViewBag.Email = user.Email;
         ViewBag.ProfilePicturePath = user.ProfilePicture;

         return View();
      }

      public ActionResult SetProfilePicture()
      {
         var user = db.Accounts.Find(User.Identity.Name);
         ViewBag.Name = user.FirstName[0] + user.LastName[0];
         ViewBag.ProfilePicturePath = user.ProfilePicture;

         return View();
      }

      [HttpPost]
      public ActionResult SetProfilePicture(HttpPostedFileBase file)
      {
         if (file != null && file.ContentLength > 0)
         {
            var user = db.Accounts.Find(User.Identity.Name);

            // Extract file name from whatever was posted by browser
            var fileName = Path.GetFileName(file.FileName);

            var newFileName = user.Username + "_Profile" + Path.GetExtension(fileName);

            var sharedFolderPath = @"C:\inetpub\wwwroot\Images";

            // Combine the shared folder path with the new file name
            var fullPath = Path.Combine(sharedFolderPath, newFileName);

            // Save the file to the shared folder
            file.SaveAs(fullPath);

            user.ProfilePicture = "https://brew-queue.com/Images/" + newFileName;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("AccountInfo");
         }
         return View();
      }

      public ActionResult DeleteAccount()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         return View();
      }

      [HttpPost]
      public ActionResult DeleteAccount(DeleteAccountEntry entry)
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         Account user = db.Accounts.Find(User.Identity.Name);

         string hashedPassword = HashPassword(entry.Password);

         if (user.Password == hashedPassword && user.Username == entry.Username)
         {
            //remove user from queue if in any
            Queue user2 = db.Queues.Find(user.Username);
            if (user2 != null)
            {
               RemoveUserFromQueue(user2);
            }

            db.Accounts.Remove(user);
            db.SaveChanges();

            FormsAuthentication.SignOut();

            return RedirectToAction("MyAccount", "Home");
         }

         ViewBag.Message = "Incorrect Credentials";
         return View();
      }

      public ActionResult EditAccount()
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }
         Account user = db.Accounts.Find(User.Identity.Name);
         ViewBag.FirstName = user.FirstName;
         ViewBag.LastName = user.LastName;
         ViewBag.Phone = user.Phone;
         return View();
      }

      [HttpPost]
      public ActionResult EditAccount(AccountEditEntry entry)
      {
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

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
         if (User.Identity.Name == "")
         {
            return RedirectToAction("MyAccount", "Home");
         }

         //check if password fits constraint
         var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[1234567890!?@#$%^&*()_+]).{6,15}$");
         if (!passwordRegex.IsMatch(entry.NewPassword))
         {
            ViewBag.Message = "Password must be 6 - 15 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.";
            return View(entry);
         }

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

      private void RemoveUserFromQueue(Queue user)
      {
         int quantity = user.Quantity;
         int position = user.Position;
         bool oldValidateOnSaveEnabled = db.Configuration.ValidateOnSaveEnabled;

         try
         {
            db.Configuration.ValidateOnSaveEnabled = false;

            // Check if user exists before removing
            var existingUser = db.Queues.Find(user.Username);
            if (existingUser != null)
            {
               // Remove user from queue
               db.Queues.Remove(existingUser);

               // Update positions of remaining users
               var remainingUsers = db.Queues.Where(q => q.Bar == user.Bar && q.Position > position).ToList();
               foreach (var q in remainingUsers)
               {
                  q.Position -= quantity;
               }

               // Save all changes at once
               db.SaveChanges();
            }
         }
         finally
         {
            db.Configuration.ValidateOnSaveEnabled = oldValidateOnSaveEnabled;
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