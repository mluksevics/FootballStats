using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsIO
{
    class OutputTopRefs
    {
        public static List<RefereeOutput> TopReferees()
        {
            //veidojam List priekš datu izvades
            var outputList = new List<RefereeOutput>();

            using (var context = new StatsDB.statsEntities())
            {
                //selektējam visas komandas no datubazes
                var allReferees = from p in context.referees
                                  select p;

                foreach (var rCurrent in allReferees)
                {
                    var rOutput = new RefereeOutput();
                    rOutput.Vards = rCurrent.name;
                    rOutput.Uzvards = rCurrent.surname;

                    //mainīgie, lai saskaitītu, cik kopā spēles, noraidījumi un spēles laiks tiesnesim
                    int totalGames = 0, totalGoals = 0, totalPenalties = 0, totalTime = 0, total11mShots = 0;

                    //apskatu visas spēles un apstādāju tās, kurās ir darbojies tiesnesis
                    var games = from g in context.games select g;
                    foreach (var game in games.ToList())
                    {
                        if (game.referee.id == rCurrent.id ||
                            game.referee1.id == rCurrent.id ||
                            game.referee2.id == rCurrent.id)
                        {
                            totalGames++;

                            totalGoals += (int)game.goals_team1;
                            totalGoals += (int)game.goals_team2;

                            var gamePenalties = from p in context.penalties where p.game1.id == game.id select p;
                            totalPenalties += gamePenalties.Count();

                            totalTime += (int)game.duration;

                            var penaltyShots = from g in context.goals
                                               where g.game1.id == game.id &&
                                               g.type == "J"
                                               select g;
                            total11mShots += penaltyShots.Count();

                        }
                    }

                    rOutput.Laiks_Laukuma = TimeSpan.FromSeconds(totalTime);
                    rOutput.Sodi = totalPenalties;
                    rOutput.Varti = totalGoals;
                    rOutput.SpelesKopa = totalGames;
                    rOutput.Soda11m = total11mShots;
                    rOutput.VidSodiSpele = Math.Round((double)totalPenalties / (double)totalGames, 2);
                    rOutput.VidVartiSpele = Math.Round((double)totalGoals / (double)totalGames, 2);
                    rOutput.VidLaiks1sodam = TimeSpan.FromSeconds(totalTime / totalPenalties);
                    outputList.Add(rOutput);
                }

            }

            //sakārtojam listi priekš attēlošanas
            var outputList_sorted = outputList
                .OrderByDescending(r => r.VidSodiSpele)
                .ToList();

            Utilities.Logger.Log("Tiesnešu statistika nodota attēlošanai.");
            return outputList_sorted;
        }

        public class RefereeOutput
        {
            public RefereeOutput()
            {

            }

            public String Vards { get; set; }
            public String Uzvards { get; set; }
            public double VidSodiSpele { get; set; }
            public double VidVartiSpele { get; set; }
            public TimeSpan VidLaiks1sodam { get; set; }

            public int Soda11m { get; set; }
            public int Sodi { get; set; }
            public int Varti { get; set; }
            public int SpelesKopa { get; set; }
            public TimeSpan Laiks_Laukuma { get; set; }


        }

    }
}
