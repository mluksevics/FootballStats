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
    
    public partial class players_games
    {
        public long id { get; set; }
        public Nullable<long> game_id { get; set; }
        public Nullable<long> player_id { get; set; }
        public Nullable<long> change_on { get; set; }
        public Nullable<long> change_off { get; set; }
        public Nullable<bool> startCrew { get; set; }
    
        public virtual game game { get; set; }
        public virtual player player { get; set; }
    }
}
