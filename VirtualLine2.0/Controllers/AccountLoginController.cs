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
using System.Net.Mail;

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

      public ActionResult ResetEmailSent()
      {
         return View();
      }
      public ActionResult ForgotPassword(AccountLoginEntry entry)
      {
         return View();
      }

      public ActionResult ResetSuccessful()
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

            // Check if the user exists with the provided username/email and hashed password
            var user = context.Accounts.FirstOrDefault(u => (u.Username == entry.Username || u.Email == entry.Username) && u.Password == hashedPassword);

            if (user != null)
            {
               FormsAuthentication.SetAuthCookie(user.Username, false);
               return RedirectToAction("AccountInfo", "Account");
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

      [HttpPost]
      public ActionResult ForgotPassword(string email)
      {
         var user = db.Accounts.FirstOrDefault(u => u.Email == email);
         if (user != null)
         {
            // Generate token
            var token = GeneratePasswordResetToken();
            token = HttpUtility.UrlEncode(token);
            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(6);            
            db.SaveChanges();

            // Send email
            SendResetEmail(user.Email, token);
            return View("ResetEmailSent"); // Inform the user that an email has been sent
         }

         ViewBag.Message = "An account with email does not exist";
         return View(email);
      }

      private void SendResetEmail(string email, string token)
      {
         var resetLink = "http://brew-queue.com/AccountLogin/ChangePassword?token=" + token;

         var body = $"Please reset your password by clicking on this link: {resetLink}";

         var smtpClient = new SmtpClient("mail.smtp2go.com")
         {
            Port = 2525, // 587 or 465
            Credentials = new NetworkCredential("brew-queue.com", "HappyValley2023!"),
            EnableSsl = true,
         };

         var mailMessage = new MailMessage
         {
            From = new MailAddress("admin@brew-queue.com"),
            Subject = "Password Reset",
            Body = body,
            IsBodyHtml = true,
         };

         mailMessage.To.Add(email);

         smtpClient.Send(mailMessage);
      }

      [HttpGet]
      public ActionResult ResetPassword(string token)
      {
         ViewBag.Token = token;
         return View();
      }

      [HttpPost]
      public ActionResult ResetPassword(string token, string newPassword, string newPasswordConfirmation)
      {
         token = HttpUtility.UrlEncode(token);
         var user = db.Accounts.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);

         if (user != null && newPassword == newPasswordConfirmation)
         {
            user.Password = HashPassword(newPassword);
            user.ResetToken = null; // Clear the token in the database
            user.ResetTokenExpires = null; // Clear the expiration time
            db.SaveChanges();

            return RedirectToAction("ResetSuccessful");
         }


         ViewBag.Message = "Password reset failed. Either the passwords do not match or your session has expired.";
         return View(); // Return the same view with the error message
      }

      private string GeneratePasswordResetToken()
      {
         using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
         {
            var byteToken = new byte[32]; // 32 bytes will give 256 bits of randomness
            rng.GetBytes(byteToken);

            // Convert the byte array to a string (Base64 is a good way to turn a byte array into a string)
            return Convert.ToBase64String(byteToken);
         }
      }
   }

   
}