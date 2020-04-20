namespace Employees.Web.Controllers
{
    using Employees.Services;
    using Employees.Web.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadDataFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
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

                    //BinaryReader b = new BinaryReader(file.InputStream);
                    //byte[] binData = b.ReadBytes(file.ContentLength);

                    //string textFile = System.Text.Encoding.UTF8.GetString(binData);
                    //string path = Server.MapPath("~/Files/EmployeesData.txt");
                    //using (StreamWriter sw = System.IO.File.CreateText(path))
                    //{
                    //    sw.WriteLine(textFile);
                    //}

                    EmployeeService employeeService = new EmployeeService();

                    var employees = employeeService.FillProjectHistory(data);
                    var emplooyeesCombination = employeeService.MatchCommonWorkingDays(employees, true);

                    var result = emplooyeesCombination.OrderByDescending(k => k.Value);//.OrderByDescending(k => k.Value).FirstOrDefault();

                    List<EmployeeViewModel> employeesHistory = new List<EmployeeViewModel>();
                    foreach (var entry in result)
                    {
                        string[] colleagues = Array.ConvertAll(entry.Key.Split(':'), p => p.Trim());

                        EmployeeViewModel employeeViewModel = new EmployeeViewModel
                        {
                            FirstEmployeeID = colleagues[0],
                            SecondEmployeeID = colleagues[1],
                            DaysWorked = entry.Value,
                            ProjectID = colleagues[2]
                        };

                        employeesHistory.Add(employeeViewModel);
                    }

                    return View(employeesHistory);
                }
                catch (Exception ex)

                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

            return View();
        }
    }
}