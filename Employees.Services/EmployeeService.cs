namespace Employees.Services
{
    using Employees.Data;
    using Employees.Services.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class EmployeeService : IEmployeeService
    {
        /// <summary>
        /// Reads the input lines and adds them to a list
        /// </summary>
        /// <param name="lines">All the data lines in the provided .txt file</param>
        /// <returns>Collection containing combination of projectId, employee id, start date and end date</returns>
        public IEnumerable<ProjectHistoryEntry> FillProjectHistory(IEnumerable<string> lines)
        {
            var projectsData = new List<ProjectHistoryEntry>();

            foreach (var line in lines)
            {
                try
                {
                    string[] data = GetDataFromLine(line);
                    var projectID = int.Parse(data[1]);

                    if (!DateTime.TryParse(data[3], out DateTime dateTo))
                    {
                        dateTo = DateTime.Now;
                    }
                    else
                    {
                        dateTo = ParseDateCustom(data[3]);
                    }

                    projectsData.Add(new ProjectHistoryEntry(projectID, int.Parse(data[0]), ParseDateCustom(data[2]), dateTo));
                }
                catch (Exception ex)
                {
                    // Just show the wrong lines in output window and don't take interupt flow because of errors
                    Debug.WriteLine(ex.Message);
                }
            }

            return projectsData;
        }

        /// <summary>
        /// Get the total days of employee pairs working on various projects and return only data for the top employee couple
        /// </summary>
        /// <param name="history">All the collected employment history entered from the .txt file</param>
        /// <returns>Collection of common days based on project and pair of employee ids</returns>
        public IEnumerable<EmployeePairData> GetMostCommonDaysEmployeesPair(IEnumerable<ProjectHistoryEntry> history)
        {
            var emplWorkingTogetherInProject = EmployeesWorkingTogetherInCommonProjects(history);

            // If we have entires emplId 1 - emplId 5, and emplId 5 - emplId 1, one of them is removed
            var uniqueEmployeePairs = emplWorkingTogetherInProject.Where(e => e.EmployeeId1 < e.EmployeeId2);

            var totalDays = new List<EmployeesTotalDays>();
            foreach (var entry in uniqueEmployeePairs)
            {
                // Get the current employee pair entry from the totalHours collection
                var currentPairTotalHours = totalDays.FirstOrDefault(d => d.EmployeeId1 == entry.EmployeeId1 && d.EmployeeId2 == entry.EmployeeId2);

                if (currentPairTotalHours == null)
                {
                    currentPairTotalHours = new EmployeesTotalDays(entry.EmployeeId1, entry.EmployeeId2);
                    totalDays.Add(currentPairTotalHours);
                };

                // Add days from current project to total days the employee pair worked together
                currentPairTotalHours.TotalDaysTogether += entry.DaysWorkingTogether;
            }

            var employeePairWithMostDaysHistory = totalDays.OrderByDescending(etd => etd.TotalDaysTogether).FirstOrDefault();

            // Get full project history data only for employee pair with most working days together
            var allCommonProjectsHistory = uniqueEmployeePairs
                .Where(td => td.EmployeeId1 == employeePairWithMostDaysHistory.EmployeeId1 && td.EmployeeId2 == employeePairWithMostDaysHistory.EmployeeId2);

            return allCommonProjectsHistory;
        }

        // Return the employees working together in the same project
        private IEnumerable<EmployeePairData> EmployeesWorkingTogetherInCommonProjects(IEnumerable<ProjectHistoryEntry> history)
        {
            var emplWorkingTogetherInProject = history.Join(
               history,
               e1 => e1.ProjectId,
               e2 => e2.ProjectId,
               (e1, e2) => new { e1, e2 })
               .Where(o => o.e1.EmployeeId != o.e2.EmployeeId && AreDatesOverlapping(o.e1, o.e2))
               .Select(r => new EmployeePairData(r.e1.ProjectId, r.e1.EmployeeId, r.e2.EmployeeId, GetDaysDiff(r.e1, r.e2)));

            if (!emplWorkingTogetherInProject.Any())
            {
                throw new Exception("No employee pair has worked together on the same project at the same time");
            }

            return emplWorkingTogetherInProject;
        }

        private bool AreDatesOverlapping(ProjectHistoryEntry first, ProjectHistoryEntry second)
        {
            return first.StartDate < second.EndDate && first.EndDate > second.StartDate;
        }

        private long GetDaysDiff(ProjectHistoryEntry empl1, ProjectHistoryEntry empl2)
        {
            // Get the total number of days that the employees worked together.
            // Calculates correctly even for leap years.
            var startCommonDate = new DateTime(Math.Max(empl1.StartDate.Ticks, empl2.StartDate.Ticks));
            var endCommonDate = new DateTime(Math.Min(empl1.EndDate.Ticks, empl2.EndDate.Ticks));

            // 1 is added because we need to include the end day. E.g. 10 April to 11 April means people worked together 2 days
            long daysDiff = (endCommonDate - startCommonDate).Days + 1;
            return daysDiff;
        }

        /// <summary>
        /// Helper method to handle basic validation
        /// </summary>
        /// <param name="line">One line from the provided .txt file</param>
        /// <returns>Array of the data extracted from the string</returns>
        private string[] GetDataFromLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new ArgumentNullException("Provided line is empty");
            }

            string[] data = Array.ConvertAll(line.Split(','), p => p.Trim());

            // Make sure we always have 4 parts of data
            if (data.Count() != 4)
            {
                throw new ArgumentException($"Invalid data detected: {line}");
            }

            return data;
        }

        private DateTime ParseDateCustom(string date)
        {
            var cultureInfo = new CultureInfo("bg-BG");

            // Collection of possible date formats
            var dateFormats = new[] { "M-d-yyyy", "d-M-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy-M-d", "yyyy-d-M", "yyyy-dd-MM", "yyyy-MM-dd",
                "M.d.yyyy", "d.M.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "yyyy.M.d", "yyyy.d.M", "yyyy.dd.MM", "yyyy.MM.dd",
                "M/d/yyyy", "d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/M/d", "yyyy/d/M", "yyyy/dd/MM/yyyy", "yyyy/MM/dd"
            }.Union(cultureInfo.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
            DateTime dateTimeParsed = DateTime.ParseExact(date, dateFormats, cultureInfo, DateTimeStyles.AssumeLocal);

            return dateTimeParsed;
        }
    }
}