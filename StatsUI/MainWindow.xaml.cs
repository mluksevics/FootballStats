using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
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

                //apskatam katru spēli
                foreach (Spele gameXML in gamesList)
                {
                    //definēju mainīgos, lai katras komandas iesistos vārtus
                    List<StatsDB.goal> team1goals = new List<StatsDB.goal>();
                    List<StatsDB.goal> team2goals = new List<StatsDB.goal>();

                    //Vai šāda komanda jau nav? ja ir, tad izlaižam ciklu
                    if (StatsIO.XMLtoDB.IsGameDuplicate(context, gameXML) == true) continue;

                    //apskatam katru komandu
                    List<StatsDB.team> gameTeams = StatsIO.XMLtoDB.AddTeams(context, gameXML);

                    //apskatam galveno tiesnesi
                    StatsDB.referee mainReferee = StatsIO.XMLtoDB.AddMainReferee(context, gameXML);

                    //apskatam katru līnijtiesnesi
                    List<StatsDB.referee> lineReferees = StatsIO.XMLtoDB.AddLineReferees(context, gameXML);

                    //Apstrādājam spēles datus
                    StatsDB.game game = StatsIO.XMLtoDB.AddGame(gameXML, gameTeams, mainReferee, lineReferees);

                    //ierakstam splēli. Šis automātiski ieraksta saistītos laukus - tiesnešus, komandas
                    context.games.Add(game);


                    //apstrādājam datus katrā no komandām
                    for (int i = 0; i < gameXML.Komanda.Count(); i++)
                    {
                        SpeleKomanda teamXML = gameXML.Komanda[i];
                        StatsDB.team teamDB = gameTeams[i];

                        //Apstrādāju visus komandas spēlētāju un ierakstam splētētāju-spēļu relationship
                        List<StatsDB.player> teamPlayers = StatsIO.XMLtoDB.AddPlayer(context, teamXML, teamDB);
                        List<StatsDB.players_games> gameTeamPlayers = StatsIO.XMLtoDB.AddGamesPlayers(teamPlayers, game, gameXML, teamXML);
                        foreach (StatsDB.players_games players_Games in gameTeamPlayers)
                        {
                            //atsevišķi players nerakstam - tie tiks automātiski ierakstīti
                            //jo piesaistīti players<>games tabulai
                            context.players_games.Add(players_Games); 
                        }


                        //Apstrādājam un ierakstam sodus
                        List<StatsDB.penalty> teamPenalties = StatsIO.XMLtoDB.AddPenalties(teamXML, game, teamPlayers);
                        foreach (StatsDB.penalty penalty in teamPenalties)
                        {
                            context.penalties.Add(penalty); 
                        }


                        //Apstrādājam un ierakstam vārdus un piespēles
                        List<StatsDB.goal> teamGoals = StatsIO.XMLtoDB.AddGoalsPasses(teamXML, game, teamDB, teamPlayers);
                        foreach (StatsDB.goal goal in teamGoals)
                        {
                            //pievienojam datubāzes entity framework
                            //atsevišķi piespēles nerakstam - tās ir piesaistītas vārtiem
                            //un tiks automātiski ierakstītas
                            context.goals.Add(goal);                           

                            //pievienojam vārtu "listei" kādai no komandām.
                            if (i == 0) { team1goals.Add(goal); }
                            if (i == 1) { team2goals.Add(goal); }
                        }

                    //beidzas cikls caur komandām
                    }

                    //aprēķinam punktus katrai no komandām
                    int[] scores = Utilities.ScoreCalc.getScores(team1goals, team2goals);
                    game.points_team1 = scores[0];
                    game.points_team2 = scores[1];

                    //saglabājam visus spēles datus
                    context.SaveChanges();

                //beidzas cikls caur spēlēm
                }

            }

        }


    }
}
