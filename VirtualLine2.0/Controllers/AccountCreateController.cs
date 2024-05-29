using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net.Mail;
using VirtualLine2._0.Models;

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
      private readonly queueDBEntities3 db = new queueDBEntities3();

      private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
      private static readonly Regex PhoneRegex = new Regex(@"^\d{3}-\d{3}-\d{4}$", RegexOptions.Compiled);
      private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[1234567890!@#$%^&*()_+]).{6,15}$", RegexOptions.Compiled);
      private static readonly Regex UsernameRegex = new Regex(@"^\w+$", RegexOptions.Compiled); // Letters, numbers, underscores only, no spaces

      // GET: AccountCreate
      public ActionResult CreateAccount() => View();

      public ActionResult Confirmation() => View();

      private static string HashPassword(string password)
      {
         using (var sha256 = SHA256.Create())
         {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
         }
      }

      [HttpPost]
      public async Task<ActionResult> CreateAccount(AccountCreateEntry entry)
      {
         string phone = entry.phonePart1 + entry.phonePart2 + entry.phonePart3;

         if (entry.Password != entry.ConfirmPassword)
         {
            entry.Password = "";
            entry.ConfirmPassword = "";
            SetViewBags(entry, "The passwords do not match");
            return View(entry);
         }

         if (await db.Accounts.AnyAsync(u => u.Username == entry.Username) || !UsernameRegex.IsMatch(entry.Username))
         {
            string message = "Username already exists"; 

            if (!UsernameRegex.IsMatch(entry.Username))
            {
               message = "Username can only contain letters, numbers, and underscores.";               
            }
            entry.Username = "";
            SetViewBags(entry, message, true);
            return View(entry);
         }

         if (await db.Accounts.AnyAsync(u => u.Email == entry.Email) || !EmailRegex.IsMatch(entry.Email))
         {
            string message = "An account with this email already exists";

            if (!EmailRegex.IsMatch(entry.Email))
            {
               message = "Invalid email format.";
            }

            entry.Email = "";
            SetViewBags(entry, message);
            
            return View(entry);
         }

         string fullPhone = $"{entry.phonePart1}-{entry.phonePart2}-{entry.phonePart3}";

         if (await db.Accounts.AnyAsync(u => u.Phone == phone) || !PhoneRegex.IsMatch(fullPhone))
         {
            string message = "An account with this phone number already exists";
            
            if (!PhoneRegex.IsMatch(fullPhone))
            {
               message = "Invalid phone number format";
            }

            entry.phonePart1 = "";
            entry.phonePart2 = "";
            entry.phonePart3 = "";
            SetViewBags(entry, message);

            return View(entry);
         }

         if (!PasswordRegex.IsMatch(entry.Password))
         {
            entry.Password = "";
            entry.ConfirmPassword = "";
            SetViewBags(entry, "Password does not fit the required criteria.");
            return View(entry);
         }

         var hashedPassword = HashPassword(entry.Password);
         var code = GenerateVerificationCode();
         var VerifyCodeExpires = DateTime.UtcNow.AddHours(1);

         var v = new Verification
         {
            Code = code,
            CodeExpires = VerifyCodeExpires,
            Username = entry.Username,
            Phone = phone,
            Password = hashedPassword,
            Email = entry.Email,
            FirstName = entry.FirstName,
            LastName = entry.LastName
         };

         db.Verifications.Add(v);

         try
         {
            await db.SaveChangesAsync();
         }
         catch (DbEntityValidationException)
         {
            ViewBag.Message = "Invalid email or phone format";
            return View(entry);
         }

         await SendResetEmail(v.Email, code);

         return RedirectToAction("VerifyEmail", new { email = entry.Email });
      }

      [HttpGet]
      public ActionResult VerifyEmail(string email)
      {
         ViewBag.Email = email;
         return View();
      }

      [HttpPost]
      public async Task<ActionResult> VerifyEmail(string email, string UserCode)
      {
         var v = await db.Verifications.FirstOrDefaultAsync(e => e.Email == email && e.Code == UserCode);
         if (v != null && v.CodeExpires > DateTime.Now)
         {
            var accountUser = new Account
            {
               Username = v.Username,
               Phone = v.Phone,
               Password = v.Password,
               Email = v.Email,
               FirstName = v.FirstName,
               LastName = v.LastName,
            };

            db.Accounts.Add(accountUser);
            db.Verifications.Remove(v);
            await db.SaveChangesAsync();

            return RedirectToAction("Confirmation", "AccountCreate");
         }

         ViewBag.Email = email;
         ViewBag.Message = "Invalid verification code or code expired";
         return View();
      }

      private async Task SendResetEmail(string email, string code)
      {
         var body = $"Your code is {code}";

         using (var smtpClient = new SmtpClient("mail.smtp2go.com")
         {
            Port = 2525,
            Credentials = new NetworkCredential("brew-queue.com", "HappyValley2023!"),
            EnableSsl = true,
         })
         {
            var mailMessage = new MailMessage
            {
               From = new MailAddress("admin@brew-queue.com"),
               Subject = "Verify Email",
               Body = body,
               IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
         }
      }

      private static string GenerateVerificationCode()
      {
         var rnd = new Random();
         return new string(Enumerable.Range(0, 6).Select(_ => rnd.Next(10).ToString()[0]).ToArray());
      }

      private void SetViewBags(AccountCreateEntry entry, string message, bool clearUsername = false)
      {
         ViewBag.Username = clearUsername ? "" : entry.Username;
         ViewBag.Password = entry.Password;
         ViewBag.PasswordConfirmation = entry.ConfirmPassword;
         ViewBag.FirstName = entry.FirstName;
         ViewBag.LastName = entry.LastName;
         ViewBag.Phone1 = entry.phonePart1;
         ViewBag.Phone2 = entry.phonePart2;
         ViewBag.Phone3 = entry.phonePart3;
         ViewBag.Email = entry.Email;
         ViewBag.Message = message;
      }
   }
}