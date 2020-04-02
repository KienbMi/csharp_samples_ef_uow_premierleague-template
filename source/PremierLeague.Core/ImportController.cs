using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        const string Filename = "PremierLeague.csv";
        const char Separator = ';';
        static Dictionary<string, Team> teams;

        public static IEnumerable<Game> ReadFromCsv()
        {
            string filePath = MyFile.GetFullNameInApplicationTree(Filename);
            teams = new Dictionary<string,Team>();
            List<Game> games = new List<Game>();

            if (File.Exists(filePath) == false)
            {
                throw new Exception("File does not exist");
            }

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            foreach (string line in lines)
            {
                string[] data = line.Split(Separator);
                //1;Manchester United;Tottenham Hotspur;1;0
                int round = int.Parse(data[0]);
                string homeTeamName = data[1];
                string guestTeamName = data[2];
                int homeGoals = int.Parse(data[3]);
                int guestGoals = int.Parse(data[4]);
                Team homeTeam = GetTeam(homeTeamName);
                Team awayTeam = GetTeam(guestTeamName);

                Game newGame = new Game
                {
                    Round = round,
                    HomeTeam = homeTeam,
                    GuestTeam = awayTeam,
                    HomeGoals = homeGoals,
                    GuestGoals = guestGoals
                };
                games.Add(newGame);
            }
            return games;
        }

        private static Team GetTeam(string teamName)
        {
            Team team;

            if (teams.TryGetValue(teamName, out team) == false)
            {
                team = new Team
                {
                    Name = teamName
                };
                teams[teamName] = team;
            }
            return team;
        }
    }
}
