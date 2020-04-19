namespace Employees.Console
{
    using System.Linq;
    using System;
    using Employees.Services;

    public class Program
    {
        private static readonly string textFile = @"Data\\EmployeesData.txt";

        public static void Main(string[] args)
        {
            EmployeeService employeeService = new EmployeeService();
            var employees = employeeService.FillProjectHistory(textFile);
            var emplooyeesCombination = employeeService.MatchCommonWorkingDays(employees, false);

            var result = emplooyeesCombination.OrderByDescending(k => k.Value).FirstOrDefault();

            string[] colleagues = Array.ConvertAll(result.Key.Split(':'), p => p.Trim());
            Console.WriteLine($"Colleague with ID {colleagues[0]} has worked together with colleague with ID {colleagues[1]}" +
                $" for total of {result.Value} days.");
        }
    }
}
