using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StatsUI.Utilities
{
   public static class Logger
    {
        public static void Log(String msg)
        {
            Names.Add(msg);
        }

        //Lauks, kuru listbox lieto, lai ģenerētu log.
       public static ObservableCollection<string> Names = new ObservableCollection<string>() { "Program started!"};

}
}
