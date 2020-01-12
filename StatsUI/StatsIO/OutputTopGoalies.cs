using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsIO
{
    class OutputTopGoalies
    {
        public static List<GoalieOutput> TopGoalies(String teamSelected = "0")
        {
            //veidojam List priekš datu izvades
            var outputList = new List<GoalieOutput>();

            using (var context = new StatsDB.statsEntities())
            {
                //selektējam vārsarigus no datubazes
                List<StatsDB.player> allGoalies = new List<StatsDB.player>();
                if (teamSelected == "0")
                {
                    allGoalies = (from p in context.players
                                  where p.position == "V"
                                  select p).ToList();
                }
                else
                {
                    allGoalies = (from p in context.players
                                  where p.team1.name == teamSelected &&
                                  p.position == "V"
                                  select p).ToList();
                }

                //savācam datus katram vārtsargam
                foreach (var vCurrent in allGoalies)
                {
                    var vOutput = new GoalieOutput();
                    vOutput.Vards = vCurrent.name;
                    vOutput.Uzvards = vCurrent.surname;
                    vOutput.Nr = (int)vCurrent.no;
                    vOutput.Komanda = vCurrent.team1.name;

                    //
                    //aprēķinam ielaistos vārdus
                    //
                    int lostGoals = 0;

                    //selektējam visas spēles, kurās vārtsargs piedalījies
                    var games = (from p in context.players_games
                                where p.player.id == vCurrent.id &&
                                p.change_on  != -1 &&
                                p.change_off != -1
                                select p.game).Distinct();
                    
                    foreach(StatsDB.game game in games)
                    {
                        //selektējam visus vārtus spēlēs, kurās vārtsargs piedalījies
                        var goalsInGame = from g in context.goals
                                          where g.game1.id == game.id
                                          select g;

                        var playerGame = from pg in context.players_games
                                         where pg.game.id == game.id &&
                                         pg.player.id == vCurrent.id
                                         select pg;
                        
                        //nosakam, kurā brīdī vārstargs ir bijis laukumā
                        int changeOn = (int)playerGame.First().change_on;
                        int changeOff = (int)playerGame.First().change_off;

                        foreach (StatsDB.goal goal in goalsInGame)
                        {
                            // apskatu tikai pretinieku iesistos vārtus!
                            if (goal.team1.id == vCurrent.team1.id) continue;

                            //pārbaudu, vai vārtsargs bijis laukumā, kad vārti ielaisti
                            //ja ir, tad vārti ir ielaisti
                            if (changeOn <= goal.time && goal.time <= changeOff) lostGoals++;
                        }
                    } 
                    vOutput.Varti = lostGoals;
                    //beidzam aprēķināt ielaistos vārtus

                    //aprēķinu kopējo laiku laukumā
                    int totalSeconds = (int)vCurrent
                        .players_games
                        .Sum(item => (item.change_off - item.change_on));
                    vOutput.Laiks_Laukuma = TimeSpan.FromSeconds(totalSeconds);

                    //aprēķinu spēles spēlētas kopā
                    var gameCount = from p in context.players_games
                                    where p.player.id == vCurrent.id &&
                                    p.change_on != -1 &&
                                    p.change_off != -1
                                    select p;
                    int totalGames = gameCount.Count();
                    vOutput.SpelesKopa = totalGames;

                    //aprēķinu spēles pamatsastāvā
                    var gamesStartCrew = from p in context.players_games
                                         where p.change_on == 0 &&
                                         p.player.id == vCurrent.id
                                         select p;
                    vOutput.SpelesPamatsast = gamesStartCrew.Count();

                    //aprēķinu vidēji ielaistos vārtus spēlē un ik pa cik minūtēm ielaiž vārtus
                    vOutput.VidVartiSpele = Math.Round((double)lostGoals / (double)totalGames, 2);
                    vOutput.VidMinutes1vartiem = TimeSpan.FromSeconds(totalSeconds / lostGoals);

                    outputList.Add(vOutput);
                }

            }

            //sakārtojam listi priekš attēlošanas
            var outputList_sorted = outputList
                .OrderBy(c => c.VidVartiSpele)
                .ToList();

            Utilities.Logger.Log("Vārtsargu statistika nodota attēlošanai.");
            return outputList_sorted;
        }

        public class GoalieOutput
        {
            public GoalieOutput()
            {

            }

            public String Vards { get; set; }
            public String Uzvards { get; set; }
            public int Nr { get; set; }
            public String Komanda { get; set; }
            public double VidVartiSpele { get; set; }
            public TimeSpan VidMinutes1vartiem { get; set; }
            public int Varti { get; set; }
            public TimeSpan Laiks_Laukuma { get; set; }
            public int SpelesKopa { get; set; }
            public int SpelesPamatsast { get; set; }

        }


    }


}
