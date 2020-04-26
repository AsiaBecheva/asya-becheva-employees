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
            var result = employeeService.GetMostCommonDaysEmployeesPair(employees);
            var employeeId1 = result.FirstOrDefault()?.EmployeeId1;
            var employeeId2 = result.FirstOrDefault()?.EmployeeId2;
            Console.WriteLine($"Colleague with ID {employeeId1} has worked together with colleague with ID {employeeId2}" +
                $" for total of {result.Sum(r => r.DaysWorkingTogether)} days.");
        }
    }
}
