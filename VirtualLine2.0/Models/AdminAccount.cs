//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VirtualLine2._0.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AdminAccount
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string AdminUsername { get; set; }
        public string City { get; set; }
        public string ResetToken { get; set; }
        public Nullable<System.DateTime> ResetTokenExpires { get; set; }
        public string ProfilePicture { get; set; }
        public string BannerPicture { get; set; }
    }
}
