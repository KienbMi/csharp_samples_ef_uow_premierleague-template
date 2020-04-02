using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;

namespace PremierLeague.Core.Contracts
{
    public interface ITeamRepository
    {
        IEnumerable<Team> GetAllWithGames();
        IEnumerable<Team> GetAll();
        void AddRange(IEnumerable<Team> teams);
        Team Get(int teamId);
        void Add(Team team);
        (Team Team, int Goals) GetTeamWithMostGoals();
        (Team Team, int Goals) GetTeamWithMostAwayGoals();
        (Team Team, int Goals) GetTeamWithMostHomeGoals();
        (Team Team, int GoalRatio) GetTeamWithBestGoalRatio();
        TeamStatisticDto[] GetTeamStatistics();
        TeamTableRowDto[] GetTeamTableRow();
    }
}