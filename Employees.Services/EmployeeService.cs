﻿namespace Employees.Services
{
    using Employees.Data;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class EmployeeService
    {
        public Dictionary<string, long> MatchCommonWorkingDays(Dictionary<string, List<EmployeeHistory>> history, bool getTotalWorkingDays)
        {
            Dictionary<string, long> employeesHours = new Dictionary<string, long>();
            foreach (var entry in history)
            {
                foreach (var empl1 in entry.Value)
                {
                    foreach (var empl2 in entry.Value)
                    {
                        if (empl1.EmpId == empl2.EmpId)
                        {
                            continue;
                        }

                        bool overlap = empl1.StartDate < empl2.EndDate && empl1.EndDate > empl2.StartDate;

                        if (overlap)
                        {
                            AddEmployeesDaysEntry(employeesHours, empl1, empl2, entry.Key, getTotalWorkingDays);
                        }
                    }
                }
            }

            return employeesHours;
        }

        private void AddEmployeesDaysEntry(
            Dictionary<string, long> employeesHours,
            EmployeeHistory empl1, 
            EmployeeHistory empl2, 
            string projectId,
            bool getProjectTotalDays
            )
        {
            var startCommonDate = new DateTime(Math.Max(empl1.StartDate.Ticks, empl2.StartDate.Ticks));
            var endCommonDate = new DateTime(Math.Min(empl1.EndDate.Ticks, empl2.EndDate.Ticks));
            int daysDiff = (endCommonDate - startCommonDate).Days;

            var employeesCombination = empl1.EmpId + ":" + empl2.EmpId;
            if (getProjectTotalDays)
            {
                employeesCombination += ":" + projectId;
            }
            if (!employeesHours.ContainsKey(employeesCombination))
            {
                employeesHours[employeesCombination] = 0;
            }

            employeesHours[employeesCombination] += daysDiff;
        }

        public Dictionary<string, List<EmployeeHistory>> FillProjectHistory(IEnumerable<string> lines)
        {
            Dictionary<string, List<EmployeeHistory>> history = new Dictionary<string, List<EmployeeHistory>>();

            foreach (var line in lines)
            {
                string[] parts = Array.ConvertAll(line.Split(','), p => p.Trim());

                if (!DateTime.TryParse(parts[3], out DateTime dateTo))
                {
                    dateTo = DateTime.Now;
                }
                else
                {
                    dateTo = ParseDateCustom(parts[3]);
                }

                var projectID = parts[1];

                if (!history.ContainsKey(projectID))
                {
                    history[projectID] = new List<EmployeeHistory>();
                }

                history[projectID].Add(new EmployeeHistory
                {
                    EmpId = parts[0],
                    StartDate = ParseDateCustom(parts[2]),
                    EndDate = dateTo
                });
            }

            return history;
        }

        private DateTime ParseDateCustom(string date)
        {
            var dateTimeParsed = new DateTime();
            var cultureInfo = new CultureInfo("bg-BG");
            date = date.Trim();

            var dateFormats = new[] { "M-d-yyyy", "d-M-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy-M-d", "yyyy-d-M", "yyyy-dd-MM", "yyyy-MM-dd",
                "M.d.yyyy", "d.M.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "yyyy.M.d", "yyyy.d.M", "yyyy.dd.MM", "yyyy.MM.dd",
                "M/d/yyyy", "d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/M/d", "yyyy/d/M", "yyyy/dd/MM/yyyy", "yyyy/MM/dd"
            }.Union(cultureInfo.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
            dateTimeParsed = DateTime.ParseExact(date, dateFormats, cultureInfo, DateTimeStyles.AssumeLocal);

            return dateTimeParsed;
        }
    }
}
