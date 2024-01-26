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
         string phone = entry.phonePart1 + entry.phonePart2 + entry.phonePart3;

         if (entry.Password != entry.ConfirmPassword)
         {
            ViewBag.Username = entry.Username;
            ViewBag.Password = "";
            ViewBag.PasswordConfirmation = "";
            ViewBag.FirstName = entry.FirstName;
            ViewBag.LastName = entry.LastName;
            ViewBag.Phone1 = entry.phonePart1;
            ViewBag.Phone2 = entry.phonePart2;
            ViewBag.Phone3 = entry.phonePart3;
            ViewBag.Email = entry.Email;
            ViewBag.Message = "The passwords do not match"; 
            return View(entry);
         }

         // Check if username already exists
         if (db.Accounts.Any(u => u.Username == entry.Username))
         {
            ViewBag.Username = "";
            ViewBag.Password = entry.Password;
            ViewBag.PasswordConfirmation = entry.ConfirmPassword;
            ViewBag.FirstName = entry.FirstName;
            ViewBag.LastName = entry.LastName;
            ViewBag.Phone1 = entry.phonePart1;
            ViewBag.Phone2 = entry.phonePart2;
            ViewBag.Phone3 = entry.phonePart3;
            ViewBag.Email = entry.Email;
            ViewBag.Message = "Username already exists";
            return View(entry);
         }

         // Check if email already exists and in correct format
         var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
         if (db.Accounts.Any(u => u.Email == entry.Email) || !emailRegex.IsMatch(entry.Email))
         {
            ViewBag.Username = entry.Username;
            ViewBag.Password = entry.Password;
            ViewBag.PasswordConfirmation = entry.ConfirmPassword;
            ViewBag.FirstName = entry.FirstName;
            ViewBag.LastName = entry.LastName;
            ViewBag.Phone1 = entry.phonePart1;
            ViewBag.Phone2 = entry.phonePart2;
            ViewBag.Phone3 = entry.phonePart3;
            ViewBag.Email = "";

            if (!emailRegex.IsMatch(entry.Email))
            {
               ViewBag.Message = "Password must be 6-15 characters long and contain at least one uppercase letter, one lowercase letter, and one special character.";
               return View(entry);
            }
            ViewBag.Message = "An account with this email already exists";
            return View(entry);
         }
         // Check if phone already exists

         var phoneRegex = new Regex(@"^\d{3}-\d{3}-\d{4}$");
         string fullPhone = entry.phonePart1 + "-" + entry.phonePart2 + "-" + entry.phonePart3;

         if (db.Accounts.Any(u => u.Phone == phone) || !phoneRegex.IsMatch(fullPhone))
         {
            ViewBag.Username = entry.Username;
            ViewBag.Password = entry.Password;
            ViewBag.PasswordConfirmation = entry.ConfirmPassword;
            ViewBag.FirstName = entry.FirstName;
            ViewBag.LastName = entry.LastName;
            ViewBag.Phone1 = "";
            ViewBag.Phone2 = "";
            ViewBag.Phone3 = "";
            ViewBag.Email = entry.Email;

            if (!phoneRegex.IsMatch(fullPhone))
            {
               ViewBag.Message = "Invalid phone number format";
               return View(entry);
            }
            ViewBag.Message = "An account with this phone number already exists";
            return View(entry);
         }
         //check if password fits constraint
         var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[1234567890!@#$%^&*()_+]).{6,15}$");
         if (!passwordRegex.IsMatch(entry.Password))
         {
            ViewBag.Username = entry.Username;
            ViewBag.Password = "";
            ViewBag.PasswordConfirmation = "";
            ViewBag.FirstName = entry.FirstName;
            ViewBag.LastName = entry.LastName;
            ViewBag.Phone1 = entry.phonePart1;
            ViewBag.Phone2 = entry.phonePart2;
            ViewBag.Phone3 = entry.phonePart3;
            ViewBag.Email = entry.Email;
            ViewBag.Message = "Password does not fit the required criteria.";
            return View(entry);
         }

         var hashedPassword = HashPassword(entry.Password); // Use the HashPassword method as defined earlier
         var code = GenerateVerificationCode();
         var VerifyCodeExpires = DateTime.UtcNow.AddHours(1);

         Verification v = new Verification();

         v.Code = code;
         v.CodeExpires = VerifyCodeExpires;
         v.Username = entry.Username;
         v.Phone = phone;
         v.Password = hashedPassword;
         v.Email = entry.Email;
         v.FirstName = entry.FirstName;
         v.LastName = entry.LastName;

         db.Verifications.Add(v);
         try
         {
            db.SaveChanges();
         }
         catch (DbEntityValidationException ex)
         {
            ViewBag.Message = "Invalid email or phone format";
            return View(entry);
         }

         // Send email
         SendResetEmail(v.Email, code);

         return RedirectToAction("VerifyEmail", new { email = entry.Email });
      }

      [HttpGet]
      public ActionResult VerifyEmail(string email)
      {
         ViewBag.Email = email;
         return View();
      }

      [HttpPost]
      public ActionResult VerifyEmail(string email, string UserCode)
      {
         var v = db.Verifications.FirstOrDefault(e => e.Email == email && e.Code == UserCode);
         if (v != null && v.CodeExpires > DateTime.Now)
         {
            Account accountUser = new Account
            {
               Username = v.Username,
               Phone = v.Phone,
               Password = v.Password,
               Email = v.Email,
               FirstName = v.FirstName,
               LastName = v.LastName
            };

            db.Accounts.Add(accountUser);
            db.SaveChanges();

            db.Verifications.Remove(v);
            db.SaveChanges();

            return RedirectToAction("Confirmation", "AccountCreate");
         }

         ViewBag.Email = email;
         ViewBag.Message = "Invalid verification code or code expired";
         return View();
      }

      private void SendResetEmail(string email, string code)
      {
         var body = $"Your code is " + code;
         // Code to send email

         var smtpClient = new SmtpClient("mail.smtp2go.com")
         {
            Port = 2525, // 587 or 465
            Credentials = new NetworkCredential("brew-queue.com", "HappyValley2023!"),
            EnableSsl = true,
         };

         var mailMessage = new MailMessage
         {
            From = new MailAddress("admin@brew-queue.com"),
            Subject = "Verify Email",
            Body = body,
            IsBodyHtml = true,
         };

         mailMessage.To.Add(email);

         smtpClient.Send(mailMessage);
      }

      private string GenerateVerificationCode()
      {
         String code = "";
         Random rnd = new Random();
         for (int i = 0; i < 6; i++)
         {
            int num = rnd.Next(10);
            code += num.ToString();
         }

         return code;
      }
   }

   
}