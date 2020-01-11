using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using StatsUI.StatsXML;
using StatsUI.Utilities;

namespace StatsUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            String folderPath = "C:/Users/User/Google Drive/!Studies/Year1/Programming/PD2/XMLFirstRound/";
            List<Spele> gamesList = parseXML.Read(folderPath);

            using (var context = new StatsDB.statsEntities())
            {
                // Query for all blogs with names starting with B
                var teams = from b in context.teams
                            select b;
                var aaa = teams.First();
                String teamName = aaa.name;

                teams.First().name = "update from program";
                context.SaveChanges();

                String aaaa = "aa";
            }

            String aa = "aa";
        }


    }
}
