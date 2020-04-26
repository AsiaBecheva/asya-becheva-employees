namespace Employees.Data
{
    public class EmployeesTotalDays
    {
        public EmployeesTotalDays(int employeeId1, int employeeId2)
        {
            this.EmployeeId1 = employeeId1;
            this.EmployeeId2 = employeeId2;
            this.TotalDaysTogether = 0;
        }
        public int EmployeeId1 { get; set; }
        public int EmployeeId2 { get; set; }
        public long TotalDaysTogether { get; set; }
    }
}