using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.Utilities
{
    public static class Conversion
    {
        public static int timeMMSStoSeconds(String mmss)
        {
            String[] split = mmss.Split(':');
            int minutes = Int32.Parse(split[0]);
            int seconds = Int32.Parse(split[1]);

            return (minutes * 60 + seconds);
        }

        public static DateTime dateYYYYMMDDtoDateTime(String yyyymmdd)
        {
            DateTime datetime = DateTime.ParseExact(yyyymmdd,
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
            return datetime;
        }
    }

}
