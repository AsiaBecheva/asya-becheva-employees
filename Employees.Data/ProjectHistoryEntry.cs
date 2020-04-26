using System;

namespace Employees.Data
{
    public class ProjectHistoryEntry
    {
        public ProjectHistoryEntry(int projectId, int emplId, DateTime startDate, DateTime endDate)
        {
            this.ProjectId = projectId;
            this.EmployeeId = emplId;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}