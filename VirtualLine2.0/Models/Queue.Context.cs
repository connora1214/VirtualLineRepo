﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class queueDBEntities3 : DbContext
    {
        public queueDBEntities3()
            : base("name=queueDBEntities3")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Queue> Queues { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Establishment> Establishments { get; set; }
        public virtual DbSet<EnteredUser> EnteredUsers { get; set; }
    }
}
