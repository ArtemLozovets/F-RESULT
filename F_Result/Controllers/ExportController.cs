using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using F_Result.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;

namespace F_Result.Controllers
{
    public class ExportController : Controller
    {
        private FRModel db = new FRModel();

        // GET: Projects
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        public ActionResult ExportPayments(string IDs)
        {
            var IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _payments = db.Payments.Where(x=>(IDsArray.Contains(x.id))).ToList();

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Проекты");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "ВХОДЯЩИЕ ПЛАТЕЖИ Ф1";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Название";
            ws.Cells["B4"].Value = "Руководитель";
            ws.Cells["C4"].Value = "Менеджер";
            ws.Cells["D4"].Value = "Начало Факт";
            ws.Cells["E4"].Value = "Окончание Факт";
            ws.Cells["F4"].Value = "Начало План";
            ws.Cells["G4"].Value = "Окончание План";
            ws.Cells["H4"].Value = "Примечание";
            ws.Cells["I4"].Value = "Тип";
            ws.Cells["J4"].Value = "Статус";
            ws.Cells["K4"].Value = "Расходы";
            ws.Cells["L4"].Value = "Доход";

            using (ExcelRange col = ws.Cells[4, 1, 4, 12])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 13;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }


            int row = 5;

            foreach (var item in _payments)
            {
                //ws.Cells[string.Format("A{0}", row)].Value = item.ShortName;
                //ws.Cells[string.Format("B{0}", row)].Value = item.ChiefName;
                //ws.Cells[string.Format("C{0}", row)].Value = item.ProjectManagerName;
                //ws.Cells[string.Format("D{0}", row)].Value = item.StartDateFact;
                //ws.Cells[string.Format("E{0}", row)].Value = item.EndDateFact;
                //ws.Cells[string.Format("F{0}", row)].Value = item.StartDatePlan;
                //ws.Cells[string.Format("G{0}", row)].Value = item.EndDatePlan;
                //ws.Cells[string.Format("H{0}", row)].Value = item.Desc;
                //ws.Cells[string.Format("I{0}", row)].Value = item.ProjectType;
                //ws.Cells[string.Format("J{0}", row)].Value = item.State;
                //ws.Cells[string.Format("K{0}", row)].Value = item.planBenefit;
                //ws.Cells[string.Format("L{0}", row)].Value = item.planExpand;

                row++;
            }

            using (ExcelRange col = ws.Cells[5, 4, row, 7])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].AutoFitColumns();

            string path = Server.MapPath("~/Reports/");
            string repName = "Projects_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName }, JsonRequestBehavior.AllowGet);

            //Response.Clear();
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment; filename=" + "Projects.xlsx");
            //Response.BinaryWrite(pck.GetAsByteArray());
            //Response.End();

        }

        public void EPExport()
        {
            try
            {
                var _prjs = db.Projects.ToList();
                ExcelPackage pck = new ExcelPackage();
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Проекты");

                ws.Cells["A1"].Value = "Название отчета:";
                ws.Cells["B1"].Value = "П Р О Е К Т Ы";
                ws.Cells["A2"].Value = "Дата формирования:";
                ws.Cells["B2"].Value = DateTime.Now;
                ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                ws.Cells["B1:B2"].Style.Font.Bold = true;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws.Cells["A4"].Value = "Название";
                ws.Cells["B4"].Value = "Руководитель";
                ws.Cells["C4"].Value = "Менеджер";
                ws.Cells["D4"].Value = "Начало Факт";
                ws.Cells["E4"].Value = "Окончание Факт";
                ws.Cells["F4"].Value = "Начало План";
                ws.Cells["G4"].Value = "Окончание План";
                ws.Cells["H4"].Value = "Примечание";
                ws.Cells["I4"].Value = "Тип";
                ws.Cells["J4"].Value = "Статус";
                ws.Cells["K4"].Value = "Расходы";
                ws.Cells["L4"].Value = "Доход";

                using (ExcelRange col = ws.Cells[4, 1, 4, 12])
                {
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    col.Style.Font.Size = 13;
                    col.Style.Font.Bold = true;
                    col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                }


                int row = 5;

                foreach (var item in _prjs)
                {
                    ws.Cells[string.Format("A{0}", row)].Value = item.ShortName;
                    ws.Cells[string.Format("B{0}", row)].Value = item.ChiefName;
                    ws.Cells[string.Format("C{0}", row)].Value = item.ProjectManagerName;
                    ws.Cells[string.Format("D{0}", row)].Value = item.StartDateFact;
                    ws.Cells[string.Format("E{0}", row)].Value = item.EndDateFact;
                    ws.Cells[string.Format("F{0}", row)].Value = item.StartDatePlan;
                    ws.Cells[string.Format("G{0}", row)].Value = item.EndDatePlan;
                    ws.Cells[string.Format("H{0}", row)].Value = item.Desc;
                    ws.Cells[string.Format("I{0}", row)].Value = item.ProjectType;
                    ws.Cells[string.Format("J{0}", row)].Value = item.State;
                    ws.Cells[string.Format("K{0}", row)].Value = item.planBenefit;
                    ws.Cells[string.Format("L{0}", row)].Value = item.planExpand;

                    row++;
                }

                using (ExcelRange col = ws.Cells[5, 4, row, 7])
                {
                    col.Style.Numberformat.Format = "dd/MM/yyyy";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                ws.Cells["A:AZ"].AutoFitColumns();
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + "Projects.xlsx");
                Response.BinaryWrite(pck.GetAsByteArray());
                Response.End();

                ViewData["Result"] = "Экспорт успешно завершен";
            }
            catch (Exception ex)
            {
                ViewData["Result"] = ex.Message;
            }
        }


        public ActionResult GetFile(string FName)
        {
            try
            {
                // Путь к файлу
                string file_path = Server.MapPath("~/Reports/");
                // Тип файла - content-type
                string file_type = "multipart/mixed";
                // Имя файла
                string file_name = FName;
                string full_file_name = file_path + FName;

                if (System.IO.File.Exists(full_file_name)) return File(full_file_name, file_type, file_name);
                else
                {
                    ViewBag.ErMes = "Файл " + file_name + " не найден.";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErMes = ex.Message;
                ViewBag.ErStack = ex.StackTrace;
                return View("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}