using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }

        public (Team Team, int Goals) GetTeamWithMostGoals()
        {
            var team = _dbContext.Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)
                })
                .OrderByDescending(t => t.Goals)
                .First();
                
            return Tuple.Create(team.Team, team.Goals).ToValueTuple();
        }

        public (Team Team, int Goals) GetTeamWithMostAwayGoals()
        {
            var team = _dbContext.Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.AwayGames.Sum(g => g.GuestGoals)
                })
                .OrderByDescending(t => t.Goals)
                .First();

            return Tuple.Create(team.Team, team.Goals).ToValueTuple();
        }

        public (Team Team, int Goals) GetTeamWithMostHomeGoals()
        {
            var team = _dbContext.Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.HomeGames.Sum(g => g.HomeGoals)
                })
                .OrderByDescending(t => t.Goals)
                .First();

            return Tuple.Create(team.Team, team.Goals).ToValueTuple();
        }

        public (Team Team, int GoalRatio) GetTeamWithBestGoalRatio()
        {
            var team = _dbContext.Teams
                .Select(t => new
                {
                    Team = t,
                    GoalRatio = t.HomeGames.Sum(g => g.HomeGoals) - t.HomeGames.Sum(g => g.GuestGoals)
                              + t.AwayGames.Sum(g => g.GuestGoals) - t.AwayGames.Sum(g => g.HomeGoals)

                })
                .OrderByDescending(t => t.GoalRatio)
                .First();

            return Tuple.Create(team.Team, team.GoalRatio).ToValueTuple();
        }

        public TeamStatisticDto[] GetTeamStatistics()
        {
            return _dbContext.Teams
                .Select(t => new TeamStatisticDto
                {
                    Name = t.Name,
                    AvgGoalsShotAtHome = t.HomeGames.Average(g => g.HomeGoals),
                    AvgGoalsShotOutwards = t.AwayGames.Average(g => g.GuestGoals),
                    AvgGoalsShotInTotal = t.HomeGames.Select(g => new { GoalsShot = g.HomeGoals })
                                          .Concat(t.AwayGames.Select(g => new { GoalsShot = g.GuestGoals}))
                                          .Average(_ => _.GoalsShot),
                    AvgGoalsGotAtHome = t.HomeGames.Average(g => g.GuestGoals),
                    AvgGoalsGotOutwards = t.AwayGames.Average(g => g.HomeGoals),
                    AvgGoalsGotInTotal = t.HomeGames.Select(g => new { GoalsGot = g.GuestGoals })
                                          .Concat(t.AwayGames.Select(g => new { GoalsGot = g.HomeGoals }))
                                          .Average(_ => _.GoalsGot)
                })
                .OrderByDescending(t => t.AvgGoalsShotInTotal)
                .ToArray();
         }

        public TeamTableRowDto[] GetTeamTableRow()
        {
            var teams = _dbContext.Teams
                .Select(t => new TeamTableRowDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Matches = t.AwayGames.Count() + t.HomeGames.Count(),
                    Won = t.HomeGames.Where(m => m.HomeGoals > m.GuestGoals).Count() + t.AwayGames.Where(m => m.HomeGoals < m.GuestGoals).Count(),
                    Lost = t.HomeGames.Where(m => m.HomeGoals < m.GuestGoals).Count() + t.AwayGames.Where(m => m.HomeGoals > m.GuestGoals).Count(),
                    GoalsFor = t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals),
                    GoalsAgainst = t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals)
                })
                .AsEnumerable()
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ToArray();

            int rank = 1;
            Array.ForEach(teams, t => t.Rank = rank++);

            return teams;
        }
    }
}