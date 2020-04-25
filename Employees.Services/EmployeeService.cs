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
        /// Reads the input lines and adds them to a convenient dictionary
        /// </summary>
        /// <param name="lines">All the data lines in the provided .txt file</param>
        /// <returns>dictionary with projectId as key and collection of EmployeeHistory as value</returns>
        public Dictionary<int, List<EmployeeHistory>> FillProjectHistory(IEnumerable<string> lines)
        {
            Dictionary<int, List<EmployeeHistory>> history = new Dictionary<int, List<EmployeeHistory>>();

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
                  
                    // Create new EmployeeHistory list if this is the first time the projectId appears
                    if (!history.ContainsKey(projectID))
                    {
                        history[projectID] = new List<EmployeeHistory>();
                    }

                    history[projectID].Add(
                        new EmployeeHistory(int.Parse(data[0]), ParseDateCustom(data[2]), dateTo));
                }
                catch (Exception ex)
                {
                    // Just show the wrong lines in output window and don't take interupt flow because of errors
                    Debug.WriteLine(ex.Message);
                }
            }

            return history;
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

        /// <summary>
        /// 1) Get the total hours of unique combination of "empl1Id:empl2Id:projectId"
        /// 2) Get the total hours of unique combination of "empl1Id:empl2Id"
        /// </summary>
        /// <param name="history">All the collected employment history based on project</param>
        /// <param name="getTotalWorkingDays">If true, executes 1, else executes 2</param>
        /// <returns>Total common working days between two employees. It can either be for all projects or only for specific project</returns>
        public Dictionary<string, long> MatchCommonWorkingDays(
            Dictionary<int, List<EmployeeHistory>> history, 
            bool getTotalWorkingDays)
        {
            Dictionary<string, long> employeesHours = new Dictionary<string, long>();
            foreach (var entry in history)
            {
                foreach (var empl1 in entry.Value)
                {
                    // When iterating all the employees of project history, we need to filter all entries with current employee id
                    // We don't want to have entries such as emplId1 = 10 and emplId = 10
                    var employeesWithoutCurrentOne = entry.Value.Where(e => e.EmpId != empl1.EmpId);

                    foreach (var empl2 in employeesWithoutCurrentOne)
                    {
                        // check if the two employees have days where they worked together
                        bool overlap = empl1.StartDate < empl2.EndDate && empl1.EndDate > empl2.StartDate;

                        if (overlap)
                        {
                            // Add the data if the employees worked together on a project
                            AddEmployeesDaysEntry(employeesHours, empl1, empl2, entry.Key, getTotalWorkingDays);
                        }
                    }
                }
            }

            return employeesHours;
        }

        /// <summary>
        /// Add number of days that employees worked together
        /// </summary>
        /// <param name="employeesHours">Collection of data for all hours added</param>
        /// <param name="empl1">First employee data for specific project</param>
        /// <param name="empl2">Second employee data for specific project</param>
        /// <param name="projectId">ProjectId for which the employees worked together</param>
        /// <param name="getProjectTotalDays">Flag to either get all days for all projects. Or only hours for one project</param>
        private void AddEmployeesDaysEntry(
            Dictionary<string, long> employeesHours,
            EmployeeHistory empl1, 
            EmployeeHistory empl2, 
            int projectId,
            bool getProjectTotalDays)
        {
            // Get the total number of days that the employees worked together.
            // Calculates correctly even for leap years.
            var startCommonDate = new DateTime(Math.Max(empl1.StartDate.Ticks, empl2.StartDate.Ticks));
            var endCommonDate = new DateTime(Math.Min(empl1.EndDate.Ticks, empl2.EndDate.Ticks));
            int daysDiff = (endCommonDate - startCommonDate).Days;

            // Stores the key for the dictionary in the format "emplId1:emplId2(:projId)"
            var employeesCombination = empl1.EmpId + ":" + empl2.EmpId;
            if (getProjectTotalDays)
            {
                employeesCombination += ":" + projectId;
            }
            // Add a clear counter for all the commomn days
            if (!employeesHours.ContainsKey(employeesCombination))
            {
                employeesHours[employeesCombination] = 0;
            }

            employeesHours[employeesCombination] += daysDiff;
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