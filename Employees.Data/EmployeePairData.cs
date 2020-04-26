using System;

namespace Employees.Data
{
    public class EmployeePairData
    {
        public EmployeePairData(int projectId, int employeeId1, int employeeId2, long daysWorkingTogether) { 
        
            this.ProjectId = projectId;
            this.EmployeeId1 = employeeId1;
            this.EmployeeId2 = employeeId2;
            this.DaysWorkingTogether = daysWorkingTogether;
        }

        public int ProjectId { get; set; }
        public int EmployeeId1 { get; set; }
        public int EmployeeId2 { get; set; }
        public long DaysWorkingTogether { get; set; }
    }
}