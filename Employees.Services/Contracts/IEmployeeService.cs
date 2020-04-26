namespace Employees.Services.Contracts
{
    using Employees.Data;
    using System.Collections.Generic;

    public interface IEmployeeService
    {
        IEnumerable<ProjectHistoryEntry> FillProjectHistory(IEnumerable<string> lines);

        IEnumerable<EmployeePairData> GetMostCommonDaysEmployeesPair(IEnumerable<ProjectHistoryEntry> history);
    }
}
