namespace Employees.Services.Contracts
{
    using Employees.Data;
    using System.Collections.Generic;

    public interface IEmployeeService
    {
        Dictionary<int, List<EmployeeHistory>> FillProjectHistory(IEnumerable<string> lines);

        Dictionary<string, long> MatchCommonWorkingDays(Dictionary<int, List<EmployeeHistory>> history, bool getTotalWorkingDays);
    }
}
