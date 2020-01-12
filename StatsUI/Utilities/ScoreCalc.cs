using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.Utilities
{
    class ScoreCalc
    {
        public static int[] getScores(List<StatsDB.goal> team1goals, List<StatsDB.goal> team2goals)
        {
            int[] scores = new int[2];

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

            scores[0] = team1score;
            scores[1] = team2score;
            return scores;
        }
    }
}
