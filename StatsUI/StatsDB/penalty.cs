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
    
    public partial class penalty
    {
        public long id { get; set; }
        public Nullable<long> game { get; set; }
        public Nullable<long> player { get; set; }
        public Nullable<long> time { get; set; }
    
        public virtual game game1 { get; set; }
        public virtual player player1 { get; set; }
    }
}
