using StatsUI.StatsXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsMain
{
    public static class Main
    {
        public static void WriteToDB(String folderPath)
        {
            //read XML file to object model
            List<Spele> gamesList = parseXML.Read(folderPath);

            using (var context = new StatsDB.statsEntities())
            {

                //apskatam katru spēli
                foreach (Spele gameXML in gamesList)
                {
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

                    //ierakstam splēli. Šis automātiski ieraksta saistītos laukus - tiesnešus, komandas.
                    context.games.Add(game);

                    //definēju mainīgos, lai katras komandas iesistos vārtus
                    List<StatsDB.goal> team1goals = new List<StatsDB.goal>();
                    List<StatsDB.goal> team2goals = new List<StatsDB.goal>();

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
                    int[] scores_goals = Utilities.ScoreCalc.getScores(team1goals, team2goals);
                    game.points_team1 = scores_goals[0];
                    game.points_team2 = scores_goals[1];
                    game.goals_team1 = scores_goals[2];
                    game.goals_team2 = scores_goals[3];


                    //saglabājam visus spēles datus datubāzē
                    context.SaveChanges();

                    //beidzas cikls caur spēlēm
                }

            }

        }

        public static void DeleteAllDataDB()
        {
            using (var context = new StatsDB.statsEntities())
            {
                Utilities.Logger.Log("Starting to delete data from DB!");

                try
                {
                    context.players_games.RemoveRange(context.players_games);
                    context.goals.RemoveRange(context.goals);
                    context.passes.RemoveRange(context.passes);
                    context.penalties.RemoveRange(context.penalties);
                    context.players.RemoveRange(context.players);
                    context.games.RemoveRange(context.games);
                    context.referees.RemoveRange(context.referees);
                    context.teams.RemoveRange(context.teams);
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Utilities.Logger.Log("Error!" + e.Message);
                }

                Utilities.Logger.Log("All data from DB deleted!");
                CountDBdata();


            }
        }

        public static void CountDBdata()
        {
            using (var context = new StatsDB.statsEntities())
            {
                try
                {
                    Utilities.Logger.Log("Games in DB: " + context.games.Count().ToString());
                    Utilities.Logger.Log("Teams in DB: " + context.teams.Count().ToString());
                    Utilities.Logger.Log("Players in DB: " + context.players.Count().ToString());
                    Utilities.Logger.Log("Referees in DB: " + context.referees.Count().ToString());
                    Utilities.Logger.Log("Goals in DB: " + context.goals.Count().ToString());
                    Utilities.Logger.Log("Passes in DB: " + context.passes.Count().ToString());
                    Utilities.Logger.Log("Penalties in DB: " + context.penalties.Count().ToString());
                }
                catch (Exception e)
                {
                    Utilities.Logger.Log("Error!" + e.Message);
                }

            }
        }

    }
}

