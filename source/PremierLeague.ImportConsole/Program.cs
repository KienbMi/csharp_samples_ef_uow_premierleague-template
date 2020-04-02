using ConsoleTables;
using PremierLeague.Core;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using PremierLeague.Persistence;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.ImportConsole
{
    class Program
    {
        static void Main()
        {
            PrintHeader();
            InitData();
            AnalyzeData();

            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 60));

            Console.WriteLine(
                  @"
            _,...,_
          .'@/~~~\@'.          
         //~~\___/~~\\        P R E M I E R  L E A G U E 
        |@\__/@@@\__/@|             
        |@/  \@@@/  \@|            (inkl. Statistik)
         \\__/~~~\__//
          '.@\___/@.'
            `""""""
                ");

            Console.WriteLine(new String('-', 60));
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Importiert die Ergebnisse (csv-Datei >> Datenbank).
        /// </summary>
        private static void InitData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                Log.Information("Import der Spiele und Teams in die Datenbank");

                Log.Information("Datenbank löschen");
                unitOfWork.DeleteDatabase();

                Log.Information("Datenbank migrieren");
                unitOfWork.MigrateDatabase();

                Log.Information("Spiele werden von premierleague.csv eingelesen");
                var games = ImportController.ReadFromCsv().ToArray();
                if (games.Length == 0)
                {
                    Log.Warning("!!! Es wurden keine Spiele eingelesen");
                }
                else
                {
                    Log.Debug($"  Es wurden {games.Count()} Spiele eingelesen!");

                    // TODO: Teams aus den Games ermitteln
                    var teams = games.Select(g => new
                            {
                                Team = g.HomeTeam
                            })
                        .Concat(games.Select(g => new
                            {
                                Team = g.GuestTeam
                            }))
                        .Distinct()
                        .ToArray();

                    Log.Debug($"  Es wurden {teams.Count()} Teams eingelesen!");

                    Log.Information("Daten werden in Datenbank gespeichert (in Context übertragen)");
                    unitOfWork.Games.AddRange(games);

                    Log.Information("Daten wurden in DB gespeichert!");
                    unitOfWork.SaveChanges();
                }
            }
        }

        private static void AnalyzeData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                var teamWithMostGoals = unitOfWork.Teams.GetTeamWithMostGoals();
                PrintResult(
                    $"Team mit den meisten geschossenen Toren:",
                    $"{teamWithMostGoals.Team.Name}: {teamWithMostGoals.Goals} Tore");

                var teamWithMostAwayGoals = unitOfWork.Teams.GetTeamWithMostAwayGoals();
                PrintResult(
                    $"Team mit den meisten geschossenen Auswärtstoren:",
                    $"{teamWithMostAwayGoals.Team.Name}: {teamWithMostAwayGoals.Goals} Auswärtstore");

                var teamWithMostHomeGoals = unitOfWork.Teams.GetTeamWithMostHomeGoals();
                PrintResult(
                    $"Team mit den meisten geschossenen Heimtoren:",
                    $"{teamWithMostHomeGoals.Team.Name}: {teamWithMostHomeGoals.Goals} Heimtore");

                var teamWithBestGoalRatio = unitOfWork.Teams.GetTeamWithBestGoalRatio();
                PrintResult(
                    $"Team mit den meisten geschossenen Heimtoren:",
                    $"{teamWithBestGoalRatio.Team.Name}: {teamWithBestGoalRatio.GoalRatio} Torverhältnis");

                var teamAverage = unitOfWork.Teams.GetTeamStatistics();
                PrintResult(
                     $"Team Leistung im Durchschnitt (sortiert nach durchschn. geschossene Tore pro Spiel [absteig.]:",
                     String.Empty);
                ConsoleTable
                      .From(teamAverage)
                      .Configure(o => o.NumberAlignment = Alignment.Right)
                      .Write(Format.Alternative);

                PrintResult(
                     $"Team Tabelle (sortiert nach Rang):",
                     String.Empty);
                var premierLeagueTable = unitOfWork.Teams.GetTeamTableRow();
                ConsoleTable
                        .From(premierLeagueTable)
                        .Configure(o => o.NumberAlignment = Alignment.Right)
                        .Write(Format.Alternative);
            }
        }

        /// <summary>
        /// Erstellt eine Konsolenausgabe
        /// </summary>
        /// <param name="caption">Enthält die Überschrift</param>
        /// <param name="result">Enthält das ermittelte Ergebnise</param>
        private static void PrintResult(string caption, string result)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(caption))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new String('=', caption.Length));
                Console.WriteLine(caption);
                Console.WriteLine(new String('=', caption.Length));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result);
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
