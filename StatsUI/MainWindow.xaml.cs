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
            String folderPath = "C:/Users/User/Google Drive/!Studies/Year1/Programming/PD2/XMLSecondRound/";
            List<Spele> gamesList = parseXML.Read(folderPath);

            using (var context = new StatsDB.statsEntities())
            {

                //apskatam katru spēli
                foreach (Spele gameXML in gamesList)
                {
                    //Vai šāda komanda jau nav?
                    //Datuma konvertēšana
                    DateTime datetime = DateTime.ParseExact(gameXML.Laiks,
                                        "yyyy/MM/dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None);

                    //Komandu nosaukumi
                    String teamName1 = gameXML.Komanda[0].Nosaukums;
                    String teamName2 = gameXML.Komanda[1].Nosaukums;

                    //mēģinam selektēt esošas spēles ar šādu datumu un komandām
                    var checkGames = from g in context.games
                                     where g.date == datetime &&
                                           ((g.team.name == teamName1 && g.team3.name == teamName2) ||
                                           (g.team.name == teamName2 && g.team3.name == teamName1))
                                     select g;
                    //ja ieraksts šādā datumā ar komandām jau ir, tad skippojam ierakstu
                    if (checkGames.Count() != 0) continue;


                    //..turpinam - tātad šī ir spēle,kāda mums iepriekš nav bijusi.
                    //definēju mainīgos, lai glabātu info par spēli.
                    List<StatsDB.team> gameTeams = new List<StatsDB.team>();
                    List<StatsDB.referee> ref_line = new List<StatsDB.referee>();
                    List<StatsDB.goal> team1goals = new List<StatsDB.goal>();
                    List<StatsDB.goal> team2goals = new List<StatsDB.goal>();

                    //
                    //apskatam katru komandu
                    foreach (SpeleKomanda teamXML in gameXML.Komanda)
                    {
                        StatsDB.team team;

                        //skatamies vai komanda ar šādu nosaukumu jau nav.
                        //ja ir,tad atrodam un pieškiram jau eošo komandu.
                        var checkTeams = from t in context.teams
                                         where t.name == teamXML.Nosaukums
                                         select t;
                        if (checkTeams.Count() == 0)
                        {
                            team = new StatsDB.team
                            {
                                name = teamXML.Nosaukums
                            };
                        }
                        else
                        {
                            team = checkTeams.ToList().First();
                        }

                        gameTeams.Add(team);
                    }


                    //
                    //apskatam galveno tiesnesi
                    StatsDB.referee ref_main;

                    //skatamies vai šāds galvenais tiesnesis jau nav.
                    //ja ir, tad atrodam un piešķiram jau esošo tiesnesi.
                    String mainRefName = gameXML.VT[0].Vards;
                    String mainRefSurname = gameXML.VT[0].Uzvards;
                    var checkMainRef = from r in context.referees
                                     where r.name == mainRefName &&
                                           r.surname == mainRefSurname
                                       select r;

                    if (checkMainRef.Count() == 0)
                    {
                        ref_main = new StatsDB.referee
                        {
                            name = mainRefName,
                            surname = mainRefSurname
                        };
                    }
                    else
                    {
                        ref_main = checkMainRef.ToList().First();
                    }

                    //
                    //apskatam katru līnijtiesnesi
                    foreach (SpeleT refereeLine in gameXML.T)
                    {
                        StatsDB.referee referee;
                        //skatamies vai šāds līnijtiesnesis jau nav.
                        //ja ir, tad atrodam un piešķiram jau esošo tiesnesi.
                        var checkReferee = from r in context.referees
                                           where r.name == refereeLine.Vards &&
                                                 r.surname == refereeLine.Uzvards
                                      select r;

                        if (checkReferee.Count() == 0)
                        {
                            referee = new StatsDB.referee
                            {
                                name = refereeLine.Vards,
                                surname = refereeLine.Uzvards
                            };
                        }
                        else
                        {
                            referee = checkReferee.ToList().First();
                        }

                        ref_line.Add(referee);
                    }


                    //Apstrādājam spēles datus
                    StatsDB.game game = new StatsDB.game
                    {
                        place = gameXML.Vieta,
                        date = datetime,
                        spectators = Int16.Parse(gameXML.Skatitaji)
                    };
                    game.referee = ref_main;
                    game.referee1 = ref_line[0];
                    game.referee2 = ref_line[1];
                    game.team = gameTeams[0];
                    game.team3 = gameTeams[1];
                    //ierakstam spēles datus

                    //Nosakam spēles garumu (sekundes)
                    int gameDuration = 3600;
                    foreach(SpeleKomanda team in gameXML.Komanda)
                    {
                        foreach (SpeleKomandaVartiVG goal in team.Varti)
                        {
                            if (Conversion.timeMMSStoSeconds(goal.Laiks) > 3600)
                                gameDuration = Conversion.timeMMSStoSeconds(goal.Laiks);
                        }
                    }

                    game.duration = gameDuration;
                    context.games.Add(game);
                    context.SaveChanges();


                    //apstrādājam datus katrā no komandām
                    for (int i = 0; i < gameXML.Komanda.Count(); i++)
                    {
                        SpeleKomanda teamXML = gameXML.Komanda[i];
                        StatsDB.team teamDB = gameTeams[i];

                        List<StatsDB.player> teamPlayers = new List<StatsDB.player>();
                        teamPlayers.Clear();

                        //ierakstam spēlētājus
                        foreach (Speletajs playerXML in teamXML.Speletaji)
                        {
                            StatsDB.player player;
                            int playerNo = Int16.Parse(playerXML.Nr);
                            //skatamies vai šāds spēlētājs jau nav.
                            //ja ir, tad atrodam un piešķiram jau esošo tiesnesi.
                            var checkPlayer = from p in context.players
                                               where p.team1.name == teamDB.name &&
                                                     p.no == playerNo
                                              select p;

                            if (checkPlayer.Count() == 0)
                            {
                                player = new StatsDB.player
                                {
                                    name = playerXML.Vards,
                                    surname = playerXML.Uzvards,
                                    no = playerNo,
                                    position = playerXML.Loma,
                                    team1 = teamDB
                                };
                            }
                            else
                            {
                                player = checkPlayer.ToList().First();
                            }

                            //nosakām spēlētāja uznākšanu un noiešanu no laukuma
                            //skatamies, vai spēlētājs pamatsastāvā, tad sākuma laiks spēlē būs 0:00
                            int change_on = -1;             //default lietojam -1, debugging nolūkos
                            int change_off = -1;  //default ir spēles garums

                            foreach(Speletajs pamatsastavaSpel in teamXML.Pamatsastavs)
                            {
                                if (pamatsastavaSpel.Nr == playerXML.Nr)
                                {
                                    change_on = 0;
                                    change_off = gameDuration;
                                }
                                    
                            }

                            foreach(SpeleKomandaMainasMaina change in teamXML.Mainas)
                            {
                                if (change.Nr1 == playerXML.Nr)
                                {
                                    change_on = 0;
                                    change_off = Conversion.timeMMSStoSeconds(change.Laiks);
                                }
                                   

                                if (change.Nr2 == playerXML.Nr)
                                {
                                    change_on = Conversion.timeMMSStoSeconds(change.Laiks);
                                    change_off = gameDuration;
                                }
                            }

                            int penaltyCount = 0;
                            foreach(SpeleKomandaSodiSods penalty in teamXML.Sodi)
                            {
                                if(penalty.Nr == playerXML.Nr) penaltyCount++;
                                
                                if (penaltyCount == 2)
                                {
                                    change_off = Conversion.timeMMSStoSeconds(penalty.Laiks);
                                }
                            }

                            // sasaistam spēlētāju ar spēli
                            StatsDB.players_games players_Games = new StatsDB.players_games
                            {
                                game = game,
                                player = player,
                                change_on = change_on,
                                change_off = change_off
                            };

                            teamPlayers.Add(player);

                            if(context.Entry(player).State == System.Data.Entity.EntityState.Modified)
                            { context.players.Add(player); }
                            
                            context.players_games.Add(players_Games);
                            context.SaveChanges();
                        };

                        //ierakstam sodus
                        foreach(SpeleKomandaSodiSods penaltyXML in teamXML.Sodi)
                        {
                            int penaltyPlayerNo = Int16.Parse(penaltyXML.Nr);
                            int PenaltyPlayerIndex = teamPlayers.FindIndex(p => p.no == penaltyPlayerNo);
                            int penaltyTime = Utilities.Conversion.timeMMSStoSeconds(penaltyXML.Laiks);

                            StatsDB.penalty penalty = new StatsDB.penalty
                            {
                                game1 = game,
                                player1 = teamPlayers[PenaltyPlayerIndex],
                                time = penaltyTime
                            };

                            context.penalties.Add(penalty);
                            context.SaveChanges();
                        };

                        //ierakstam vārtus un piespēles
                        foreach(SpeleKomandaVartiVG goalXML in teamXML.Varti)
                        {
                            int goalPlayerNo = Int16.Parse(goalXML.Nr);
                            int GoalPlayerIndex = teamPlayers.FindIndex(p => p.no == goalPlayerNo);
                            int goalTime = Utilities.Conversion.timeMMSStoSeconds(goalXML.Laiks);

                            StatsDB.goal goal = new StatsDB.goal
                            {
                                game1 = game,
                                team1 = teamDB,
                                time = goalTime,
                                player1 = teamPlayers[GoalPlayerIndex],
                                type = goalXML.Sitiens
                            };

                            if(goalXML.P != null)
                            {
                                for (int j = 0; j < goalXML.P.Count(); j++)
                                {
                                    SpeleKomandaVartiVGP passXML = goalXML.P[j];
                                    int passPlayerNo = Int16.Parse(passXML.Nr);
                                    int PassPlayerIndex = teamPlayers.FindIndex(p => p.no == passPlayerNo);

                                    StatsDB.pass pass = new StatsDB.pass
                                    {
                                        game1 = game,
                                        team1 = teamDB,
                                        player1 = teamPlayers[PassPlayerIndex]
                                    };

                                    if (j == 0) { goal.pass5 = pass; }
                                    if (j == 1) { goal.pass4 = pass; }
                                    if (j == 2) { goal.pass = pass; }
                                }
                            }

                            //ierakstam datu bāzē
                            context.goals.Add(goal);
                            context.SaveChanges();

                            //pievienojam vārtu "listei" kādai no komandām.
                            if (i == 0) { team1goals.Add(goal); }
                            if (i == 1) { team2goals.Add(goal); }
                            
                        }
                    }

                    //aprēķinu punktus katrai komandai
                    int team1score = 0, team2score = 0;
                    int team1goalCount = team1goals.Count();
                    int team2goalCount = team2goals.Count();

                    List<StatsDB.goal> allgoals = new List<StatsDB.goal>();
                    allgoals.AddRange(team1goals);
                    allgoals.AddRange(team2goals);

                    long? latestGoalTime = allgoals.Max(x => x.time);

                    //ja nav papildlaika
                    if (latestGoalTime / 60 <= 60)
                    {
                        team1score = (team1goalCount > team2goalCount) ? 5 : 1;
                        team2score = (team1goalCount > team2goalCount) ? 1 : 5;
                    }
                    //ja ir papildlaiks
                    else
                    {
                        team1score = (team1goalCount > team2goalCount) ? 3 : 2;
                        team2score = (team1goalCount > team2goalCount) ? 2 : 3;
                    }

                    game.points_team1 = team1score;
                    game.points_team2 = team2score;
                    context.SaveChanges();

                }
                





            }

        }


    }
}
