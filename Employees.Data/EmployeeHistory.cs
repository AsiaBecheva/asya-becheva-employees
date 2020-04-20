namespace Employees.Data
{
    using System;
    public class EmployeeHistory
    {
        public EmployeeHistory(int emplId, DateTime startDate, DateTime endDate)
        {
            this.EmpId = emplId;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        public int EmpId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}