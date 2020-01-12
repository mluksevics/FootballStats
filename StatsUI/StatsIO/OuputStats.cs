using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsIO
{
    public static class OuputStats
    {
        public static List<TeamOutput> TopTeams()
        {
            //veidojam List priekš datu izvades
            var outputList = new List<TeamOutput>();

            using (var context = new StatsDB.statsEntities())
            {
                //selektējam visas komandas no datubazes
                var allTeams = from t in context.teams
                               select t;

                foreach(var tCurrent in allTeams)
                {
                    var tOutput = new TeamOutput();
                    int score = 0, pamU = 0, pamZ = 0, papU = 0, papZ = 0, goalsScored = 0, goalsLost = 0;
                    tOutput.Nosaukums = tCurrent.name;
                    tOutput.Spelataji = tCurrent.players.Count();

                    //darbojamies ar visām spēlēm, kurās komanda ir kā "team1"
                    var games1 = from g in context.games
                                 where g.team.id == tCurrent.id
                                 select g;

                    foreach (var game in games1.ToList())
                    {
                        if (game.points_team1 == 1) { pamZ++; score += 1; }
                        if (game.points_team1 == 5) { pamU++; score += 5; }
                        if (game.points_team1 == 2) { papZ++; score += 2; }
                        if (game.points_team1 == 3) { papU++; score += 3; }
                        goalsScored += (int)game.goals_team1;
                        goalsLost += (int)game.goals_team2;
                    }

                    //darbojamies ar visām spēlēm, kurās komanda ir kā "team2"
                    var games2 = from g in context.games
                                 where g.team3.id == tCurrent.id
                                 select g;

                    foreach (var game in games2.ToList())
                    {
                        if (game.points_team2 == 1) { pamZ++; score += 1; }
                        if (game.points_team2 == 5) { pamU++; score += 5; }
                        if (game.points_team2 == 2) { papZ++; score += 2; }
                        if (game.points_team2 == 3) { papU++; score += 3; }
                        goalsScored += (int)game.goals_team2;
                        goalsLost += (int)game.goals_team1;

                    }

                    tOutput.PamZ = pamZ;
                    tOutput.PamU = pamU;
                    tOutput.PapZ = papZ;
                    tOutput.PapU = papU;
                    tOutput.Punkti = score;
                    tOutput.Varti_iesisti = goalsScored;
                    tOutput.Varti_ielaisti = goalsLost;
                    tOutput.Varti_starpiba = goalsScored - goalsLost;

                    outputList.Add(tOutput);
                }

            }
            var outputList_sorted = outputList
                .OrderByDescending(c => c.Punkti)
                .ThenByDescending(n => n.Varti_starpiba)
                .ToList();

            Utilities.Logger.Log("Komandu statistika nodota attēlošanai.");

            return outputList_sorted;
        } 


    }

    public class TeamOutput
    {
        public TeamOutput()
        {

        }
        public String Nosaukums { get; set; }
        public int Punkti { get; set; }
        public int Varti_starpiba { get; set; }
        public int Varti_iesisti { get; set; }
        public int Varti_ielaisti { get; set; }
        public int PamU { get; set; }
        public int PamZ { get; set; }
        public int PapU { get; set; }
        public int PapZ { get; set; }
        public int Spelataji { get; set; }

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
        public String Laiks_Laukuma { get; set; }
        public int SpelesPamatsast { get; set; }

    }
}
