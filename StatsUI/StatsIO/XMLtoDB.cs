using StatsUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsUI.StatsIO
{
    public static class XMLtoDB
    {
        public static bool IsGameDuplicate(StatsDB.statsEntities context, Spele gameXML)
        {
            //Komandu nosaukumi
            String teamName1 = gameXML.Komanda[0].Nosaukums;
            String teamName2 = gameXML.Komanda[1].Nosaukums;

            //spēles datums
            DateTime datetime = Conversion.dateYYYYMMDDtoDateTime(gameXML.Laiks);

            //mēģinam selektēt esošas spēles ar šādu datumu un komandām
            var checkGames = from g in context.games
                             where g.date == datetime &&
                                   ((g.team.name == teamName1 && g.team3.name == teamName2) ||
                                   (g.team.name == teamName2 && g.team3.name == teamName1))
                             select g;
            
            //ja ieraksts šādā datumā ar komandām jau ir, tad spēle ir dublikāts
            if (checkGames.Count() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<StatsDB.team> AddTeams(StatsDB.statsEntities context, Spele gameXML)
        {
            List<StatsDB.team> gameTeams = new List<StatsDB.team>(); 
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

            return gameTeams;
        }

        public static StatsDB.referee AddMainReferee(StatsDB.statsEntities context, Spele gameXML)
        {
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

            return ref_main;
        }

        public static List<StatsDB.referee> AddLineReferees(StatsDB.statsEntities context, Spele gameXML)
        {
            List<StatsDB.referee> ref_line = new List<StatsDB.referee>();

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

            return ref_line;
        }

        public static StatsDB.game AddGame(
            Spele gameXML, 
            List<StatsDB.team> teams,
            StatsDB.referee mainReferee,
            List<StatsDB.referee> lineReferees
            )
        {
            DateTime datetime = Conversion.dateYYYYMMDDtoDateTime(gameXML.Laiks);

            StatsDB.game game = new StatsDB.game
            {
                place = gameXML.Vieta,
                date = datetime,
                spectators = Int16.Parse(gameXML.Skatitaji)
            };
            game.referee = mainReferee;
            game.referee1 = lineReferees[0];
            game.referee2 = lineReferees[1];
            game.team = teams[0];
            game.team3 = teams[1];
            game.duration = getGameDuration(gameXML);

            return game;
        }

        public static List<StatsDB.player> AddPlayer(StatsDB.statsEntities context, SpeleKomanda teamXML, StatsDB.team teamDB)
        {
            List<StatsDB.player> teamPlayers = new List<StatsDB.player>();

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

                teamPlayers.Add(player);
            }

            return teamPlayers;
        }


        public static List<StatsDB.players_games> AddGamesPlayers(List<StatsDB.player> players, StatsDB.game game, Spele gameXML, SpeleKomanda teamXML)
        {
            List<StatsDB.players_games> GamePlayers = new List<StatsDB.players_games>();


            foreach(StatsDB.player player in players)
            {
                //nosakām spēlētāja uznākšanu un noiešanu no laukuma
                //skatamies, vai spēlētājs pamatsastāvā, tad sākuma laiks spēlē būs 0:00
                int change_on = -1;             //default lietojam -1, debugging nolūkos
                int change_off = -1;
                bool startCrew = false;

                foreach (Speletajs pamatsastavaSpel in teamXML.Pamatsastavs)
                {
                    if (pamatsastavaSpel.Nr == player.no.ToString())
                    {
                        startCrew = true;
                        change_on = 0;
                        change_off = (int)game.duration;
                    }

                }

                foreach (SpeleKomandaMainasMaina change in teamXML.Mainas)
                {
                    if (change.Nr1 == player.no.ToString())
                    {
                        change_on = 0;
                        change_off = Conversion.timeMMSStoSeconds(change.Laiks);
                    }


                    if (change.Nr2 == player.no.ToString())
                    {
                        change_on = Conversion.timeMMSStoSeconds(change.Laiks);
                        change_off = (int)game.duration;
                    }
                }

                int penaltyCount = 0;
                foreach (SpeleKomandaSodiSods penalty in teamXML.Sodi)
                {
                    if (penalty.Nr == player.no.ToString()) penaltyCount++;

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
                    change_off = change_off,
                    startCrew = startCrew
                };

                GamePlayers.Add(players_Games);
            }

            return GamePlayers;
        }

        public static List<StatsDB.penalty> AddPenalties(SpeleKomanda teamXML, StatsDB.game game, List<StatsDB.player> teamPlayers)
        {
            List<StatsDB.penalty> teamPenalties = new List<StatsDB.penalty>();

            foreach (SpeleKomandaSodiSods penaltyXML in teamXML.Sodi)
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

                teamPenalties.Add(penalty);
            };

            return teamPenalties;
        }

        public static List<StatsDB.goal> AddGoalsPasses(SpeleKomanda teamXML, StatsDB.game game, StatsDB.team teamDB, List<StatsDB.player> teamPlayers)
        {
            List<StatsDB.goal> teamGoals = new List<StatsDB.goal>();

            foreach (SpeleKomandaVartiVG goalXML in teamXML.Varti)
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

                //Apstrādājam piespēles
                if (goalXML.P != null)
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

                        //piespēles piesaistam vārtiem - tādejādi pie vārtu ierakstīšanas, 
                        //piespēles ierakstīsies automātiski
                        if (j == 0) { goal.pass5 = pass; }
                        if (j == 1) { goal.pass4 = pass; }
                        if (j == 2) { goal.pass = pass; }
                    }
                }

                teamGoals.Add(goal);

            }

            return teamGoals;
        }

        public static int getGameDuration(Spele gameXML)
        {
            int gameDuration = 3600;
            foreach (SpeleKomanda team in gameXML.Komanda)
            {
                foreach (SpeleKomandaVartiVG goal in team.Varti)
                {
                    if (Conversion.timeMMSStoSeconds(goal.Laiks) > 3600)
                        gameDuration = Conversion.timeMMSStoSeconds(goal.Laiks);
                }
            }

            return gameDuration;
        }

    }
}
