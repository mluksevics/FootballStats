//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class goal
    {
        public long id { get; set; }
        public Nullable<long> game { get; set; }
        public Nullable<long> team { get; set; }
        public Nullable<long> time { get; set; }
        public Nullable<long> player { get; set; }
        public Nullable<long> pass1 { get; set; }
        public Nullable<long> pass2 { get; set; }
        public Nullable<long> pass3 { get; set; }
        public string type { get; set; }
    
        public virtual game game1 { get; set; }
        public virtual pass pass { get; set; }
        public virtual pass pass4 { get; set; }
        public virtual pass pass5 { get; set; }
        public virtual player player1 { get; set; }
        public virtual team team1 { get; set; }
    }
}