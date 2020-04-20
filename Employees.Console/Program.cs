namespace Employees.Console
{
    using System.Linq;
    using System;
    using Employees.Services;
    using System.IO;

    public class Program
    {
        private static readonly string textFile = @"Data\\EmployeesData.txt";

        public static void Main(string[] args)
        {
            EmployeeService employeeService = new EmployeeService();
            string[] lines = File.ReadAllLines(textFile);
            var employees = employeeService.FillProjectHistory(lines);
            var emplooyeesCombination = employeeService.MatchCommonWorkingDays(employees, false);

            var result = emplooyeesCombination.OrderByDescending(k => k.Value).FirstOrDefault();

            string[] colleagues = Array.ConvertAll(result.Key.Split(':'), p => p.Trim());
            Console.WriteLine($"Colleague with ID {colleagues[0]} has worked together with colleague with ID {colleagues[1]}" +
                $" for total of {result.Value} days.");
        }
    }
}
