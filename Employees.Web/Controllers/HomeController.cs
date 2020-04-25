namespace Employees.Web.Controllers
{
    using Employees.Services;
    using Employees.Services.Contracts;
    using Employees.Web.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    
    public class HomeController : Controller
    {
        private readonly IEmployeeService employeeService;

        public HomeController(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadDataFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                if (file.ContentType != "text/plain")
                {
                    ViewBag.ErrorMessage = "Provided file format can only be .txt!";
                    return this.View();
                }
                try
                {
                    List<string> data = new List<string>();
                    using (StreamReader reader = new StreamReader(file.InputStream))
                    {
                        while (!reader.EndOfStream)
                        {
                            data.Add(reader.ReadLine());
                        }
                    }

                    var employees = employeeService.FillProjectHistory(data);
                    var emplooyeesCombination = employeeService.MatchCommonWorkingDays(employees, true).OrderByDescending(k => k.Value);

                    List<EmployeeViewModel> employeesHistory = new List<EmployeeViewModel>();
                    foreach (var entry in emplooyeesCombination)
                    {
                        string[] colleagues = Array.ConvertAll(entry.Key.Split(':'), p => p.Trim());

                    EmployeeViewModel employeeViewModel = new EmployeeViewModel
                        {
                            FirstEmployeeID = int.Parse(colleagues[0]),
                            SecondEmployeeID = int.Parse(colleagues[1]),
                            DaysWorked = entry.Value,
                            ProjectID = int.Parse(colleagues[2])
                        };

                        employeesHistory.Add(employeeViewModel);
                    }

                    return View(employeesHistory);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"ERROR: {ex.Message}!";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "You have not specified a file to be uploaded.";
            }

            return this.View();
        }
    }
}