﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StatsUI.StatsDB
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class statsEntities : DbContext
    {
        public statsEntities()
            : base("name=statsEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<game> games { get; set; }
        public virtual DbSet<goal> goals { get; set; }
        public virtual DbSet<pass> passes { get; set; }
        public virtual DbSet<penalty> penalties { get; set; }
        public virtual DbSet<player> players { get; set; }
        public virtual DbSet<players_games> players_games { get; set; }
        public virtual DbSet<referee> referees { get; set; }
        public virtual DbSet<team> teams { get; set; }
    }
}
