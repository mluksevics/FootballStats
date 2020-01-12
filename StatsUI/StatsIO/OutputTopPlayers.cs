using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsIO
{
    class OutputTopPlayers
    {
        public static List<PlayerOutput> TopPlayers()
        {
            //veidojam List priekš datu izvades
            var outputList = new List<PlayerOutput>();

            using (var context = new StatsDB.statsEntities())
            {
                //selektējam visas komandas no datubazes
                var allPlayers = from p in context.players
                                 select p;

                foreach (var tCurrent in allPlayers)
                {
                    var pOutput = new PlayerOutput();
                    pOutput.Vards = tCurrent.name;
                    pOutput.Uzvards = tCurrent.surname;
                    pOutput.Nr = (int)tCurrent.no;
                    pOutput.Komanda = tCurrent.team1.name;
                    pOutput.Pozicija = tCurrent.position;

                    var goals = from g in context.goals where g.player1.id == tCurrent.id select g;
                    pOutput.Varti = goals.Count();

                    var passes = from p in context.passes where p.player1.id == tCurrent.id select p;
                    pOutput.Piespeles = tCurrent.passes.Count();

                    var penalties = from p in context.penalties where p.player1.id == tCurrent.id select p;
                    pOutput.Sodi = tCurrent.penalties.Count();

                    //aprēķinu kopējo laiku laukumā
                    double totalSeconds = (double)tCurrent
                        .players_games
                        .Sum(item => (item.change_off - item.change_on));
                    pOutput.Laiks_Laukuma = TimeSpan.FromSeconds(totalSeconds);

                    //aprēķinu spēles spēlētas kopā
                    var gameCount = from p in context.players_games
                                    where p.player.id == tCurrent.id &&
                                    p.change_on != -1 &&
                                    p.change_off != -1
                                    select p;
                    pOutput.SpelesKopa = gameCount.Count();

                    //aprēķinu spēles pamatsastāvā
                    var gamesStartCrew = from p in context.players_games
                                         where p.change_on == 0 &&
                                         p.player.id == tCurrent.id
                                         select p;
                    pOutput.SpelesPamatsast = gamesStartCrew.Count();

                    outputList.Add(pOutput);
                }

            }

            //sakārtojam listi priekš attēlošanas
            var outputList_sorted = outputList
                .OrderByDescending(c => c.Varti)
                .ThenByDescending(n => n.Piespeles)
                .ToList();

            Utilities.Logger.Log("Spēlētāju statistika nodota attēlošanai.");
            return outputList_sorted;
        }

        public class PlayerOutput
        {
            public PlayerOutput()
            {

            }

            public String Vards { get; set; }
            public String Uzvards { get; set; }
            public int Nr { get; set; }
            public String Komanda { get; set; }
            public String Pozicija { get; set; }
            public int Varti { get; set; }
            public int Piespeles { get; set; }
            public int Sodi { get; set; }
            public TimeSpan Laiks_Laukuma { get; set; }
            public int SpelesKopa { get; set; }
            public int SpelesPamatsast { get; set; }

        }

    }
}
