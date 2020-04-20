namespace Employees.Services
{
    using Employees.Data;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class EmployeeService
    {
        public Dictionary<int, List<EmployeeHistory>> FillProjectHistory(IEnumerable<string> lines)
        {
            Dictionary<int, List<EmployeeHistory>> history = new Dictionary<int, List<EmployeeHistory>>();

            foreach (var line in lines)
            {
                try
                {
                    string[] data = GetDataFromLine(line);

                    if (!DateTime.TryParse(data[3], out DateTime dateTo))
                    {
                        dateTo = DateTime.Now;
                    }
                    else
                    {
                        dateTo = ParseDateCustom(data[3]);
                    }

                    var projectID = int.Parse(data[1]);

                    if (!history.ContainsKey(projectID))
                    {
                        history[projectID] = new List<EmployeeHistory>();
                    }

                    history[projectID].Add(
                        new EmployeeHistory(int.Parse(data[0]), ParseDateCustom(data[2]), dateTo));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return history;
        }

        private string[] GetDataFromLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new ArgumentNullException("Provided line is empty");
            }

            string[] data = Array.ConvertAll(line.Split(','), p => p.Trim());

            if (data.Count() != 4)
            {
                throw new ArgumentException($"Invalid data detected: {line}");
            }

            return data;
        }

        public Dictionary<string, long> MatchCommonWorkingDays(
            Dictionary<int, List<EmployeeHistory>> history, 
            bool getTotalWorkingDays)
        {
            Dictionary<string, long> employeesHours = new Dictionary<string, long>();
            foreach (var entry in history)
            {
                foreach (var empl1 in entry.Value)
                {
                    var employeesWithoutCurrentOne = entry.Value.Where(e => e.EmpId != empl1.EmpId);

                    foreach (var empl2 in employeesWithoutCurrentOne)
                    {
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
            int projectId,
            bool getProjectTotalDays)
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

        private DateTime ParseDateCustom(string date)
        {
            var cultureInfo = new CultureInfo("bg-BG");
            date = date.Trim();

            var dateFormats = new[] { "M-d-yyyy", "d-M-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "yyyy-M-d", "yyyy-d-M", "yyyy-dd-MM", "yyyy-MM-dd",
                "M.d.yyyy", "d.M.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "yyyy.M.d", "yyyy.d.M", "yyyy.dd.MM", "yyyy.MM.dd",
                "M/d/yyyy", "d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/M/d", "yyyy/d/M", "yyyy/dd/MM/yyyy", "yyyy/MM/dd"
            }.Union(cultureInfo.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
            DateTime dateTimeParsed = DateTime.ParseExact(date, dateFormats, cultureInfo, DateTimeStyles.AssumeLocal);

            return dateTimeParsed;
        }
    }
}