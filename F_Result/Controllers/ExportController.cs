using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using F_Result.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using System.Linq.Dynamic;
using F_Result.Methods;
using System.Globalization;
using OfficeOpenXml.ConditionalFormatting;
using System.Drawing;

namespace F_Result.Controllers
{
    public class ExportController : Controller
    {
        private FRModel db = new FRModel();

        #region ---------------------- Форма 1 ---------------------------

        #region Входящие платежи Ф1. Экспорт в Excel

        public ActionResult ExportPaymentsF1(string IDs, string sortColumn, string sortColumnDir)
        {

            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _payments = db.Payments.Where(x => (IDsArray.Contains(x.id))).ToList();
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
            }
            else
            {
                _payments = _payments.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.id).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Входящие платежи Ф1");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ВХОДЯЩИЕ ПЛАТЕЖИ Ф1\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Тип проекта";
            ws.Cells["C4"].Value = "Руководитель";
            ws.Cells["D4"].Value = "Заказчик";
            ws.Cells["E4"].Value = "Договор";
            ws.Cells["F4"].Value = "Куратор";
            ws.Cells["G4"].Value = "Тип оплаты";
            ws.Cells["H4"].Value = "Дата оплаты";
            ws.Cells["I4"].Value = "Сумма";

            ws.Cells["A4:I4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 9])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _payments)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.Project;
                ws.Cells[string.Format("B{0}", row)].Value = item.AgrType;
                ws.Cells[string.Format("C{0}", row)].Value = item.Chief;
                ws.Cells[string.Format("D{0}", row)].Value = item.Client;
                ws.Cells[string.Format("E{0}", row)].Value = item.Agreement;
                ws.Cells[string.Format("F{0}", row)].Value = item.Manager;
                ws.Cells[string.Format("G{0}", row)].Value = item.PaymentDesc;
                ws.Cells[string.Format("H{0}", row)].Value = item.PaymentDate;
                ws.Cells[string.Format("I{0}", row)].Value = item.Payment;

                row++;
            }

            ws.Cells[string.Format("H{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("I{0}", row)].Formula = string.Format("SUM(I5:I{0})", row - 1);
            ws.Cells[string.Format("I{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 8, row, 8])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 9, row, 9])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            //ws.Cells["A:AZ"].AutoFitColumns();

            ws.Column(1).Width = 40;
            ws.Column(1).Style.WrapText = true;

            ws.Column(2).Width = 20;

            ws.Column(3).Width = 20;
            ws.Column(4).Width = 50;
            ws.Column(5).Width = 22;
            ws.Column(6).Width = 20;
            ws.Column(7).Width = 30;
            ws.Column(8).Width = 15;
            ws.Column(9).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PaymentsF1_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Исходящие платежи Ф1. Экспорт в Excel

        public ActionResult ExportActualDebitsF1(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from actualdebit in db.ActualDebit
                        join prg in db.Projects on actualdebit.ProjectId equals prg.id
                        join org in db.Organizations on actualdebit.OrganizationId equals org.id
                        join usr in db.IdentityUsers on actualdebit.UserId equals usr.Id into usrtmp
                        from usr in usrtmp.DefaultIfEmpty()
                        where (IDsArray.Contains(actualdebit.ActualDebitId))
                        select new
                        {
                            ActualDebitId = actualdebit.ActualDebitId,
                            Date = actualdebit.Date,
                            Sum = actualdebit.Sum,
                            ProjectId = actualdebit.ProjectId,
                            OrgId = org.id,
                            Appointment = actualdebit.Appointment,
                            DocNumber = actualdebit.DocNumber,
                            UserId = actualdebit.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            ProjectName = prg.ShortName,
                            ProjectType = prg.ProjectType,
                            ChiefName = prg.ChiefName,
                            ProjectManagerName = prg.ProjectManagerName,
                            StartDatePlan = prg.StartDatePlan,
                            StartDateFact = prg.StartDateFact,
                            OrgName = org.Title,
                            planBenefit = prg.planBenefit,
                            planExpand = prg.planExpand
                        }).AsEnumerable().Select(x => new ActualDebitView
                        {
                            ActualDebitId = x.ActualDebitId,
                            Date = x.Date,
                            Sum = x.Sum,
                            ProjectId = x.ProjectId,
                            ProjectName = x.ProjectName,
                            ProjectType = x.ProjectType,
                            ChiefName = x.ChiefName,
                            ProjectManagerName = x.ProjectManagerName,
                            StartDatePlan = x.StartDatePlan,
                            StartDateFact = x.StartDateFact,
                            OrganizationId = x.OrgId,
                            OrganizationName = x.OrgName,
                            Appointment = x.Appointment,
                            DocNumber = x.DocNumber,
                            UserId = x.UserId,
                            UserFN = x.UserFN,
                            planBenefit = x.planBenefit,
                            planExpand = x.planExpand
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", ActualDebitId desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.ActualDebitId).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Исходящие платежи Ф1");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ИСХОДЯЩИЕ ПЛАТЕЖИ Ф1\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Руководитель";
            ws.Cells["C4"].Value = "Организация";
            ws.Cells["D4"].Value = "Назначение";
            ws.Cells["E4"].Value = "Номер документа";
            ws.Cells["F4"].Value = "Пользователь";
            ws.Cells["G4"].Value = "Дата";
            ws.Cells["H4"].Value = "Сумма";

            ws.Cells["A4:H4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 8])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("B{0}", row)].Value = item.ChiefName;
                ws.Cells[string.Format("C{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("D{0}", row)].Value = item.Appointment;
                ws.Cells[string.Format("E{0}", row)].Value = item.DocNumber;
                ws.Cells[string.Format("F{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("G{0}", row)].Value = item.Date;
                ws.Cells[string.Format("H{0}", row)].Value = item.Sum;

                row++;
            }

            ws.Cells[string.Format("G{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("H{0}", row)].Formula = string.Format("SUM(H5:H{0})", row - 1);
            ws.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 8, row, 8])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            //ws.Cells["A:AZ"].AutoFitColumns();

            ws.Column(1).Width = 40;
            ws.Column(1).Style.WrapText = true;

            ws.Column(2).Width = 20;

            ws.Column(3).Width = 20;
            ws.Column(4).Width = 50;
            ws.Column(5).Width = 35;
            ws.Column(6).Width = 20;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "ADF1_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region План доходов Ф1. Экспорт в Excel

        public ActionResult ExportPlanCreditsF1(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from plancredit in db.PlanCredits
                        join prg in db.Projects on plancredit.ProjectId equals prg.id
                        join org in db.Organizations on plancredit.OrganizationId equals org.id
                        join usr in db.IdentityUsers on plancredit.UserId equals usr.Id into usrtmp
                        from usr in usrtmp.DefaultIfEmpty()
                        join pperiod in db.PlanningPeriods on plancredit.PeriodId equals pperiod.PlanningPeriodId
                        where (IDsArray.Contains(plancredit.PlanCreditId))
                        select new
                        {
                            PlanCreditId = plancredit.PlanCreditId,
                            Date = plancredit.Date,
                            Sum = plancredit.Sum,
                            ProjectId = plancredit.ProjectId,
                            OrgId = org.id,
                            Appointment = plancredit.Appointment,
                            UserId = plancredit.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            ProjectName = prg.ShortName,
                            ProjectType = prg.ProjectType,
                            ChiefName = prg.ChiefName,
                            ProjectManagerName = prg.ProjectManagerName,
                            StartDatePlan = prg.StartDatePlan,
                            StartDateFact = prg.StartDateFact,
                            OrgName = org.Title,
                            PeriodName = pperiod.PeriodName,
                            planBenefit = prg.planBenefit,
                            planExpand = prg.planExpand
                        }).AsEnumerable().Select(x => new PlanCreditView
                        {
                            PlanCreditId = x.PlanCreditId,
                            Date = x.Date,
                            Sum = x.Sum,
                            ProjectId = x.ProjectId,
                            ProjectName = x.ProjectName,
                            ProjectType = x.ProjectType,
                            ChiefName = x.ChiefName,
                            ProjectManagerName = x.ProjectManagerName,
                            StartDatePlan = x.StartDatePlan,
                            StartDateFact = x.StartDateFact,
                            OrganizationId = x.OrgId,
                            OrganizationName = x.OrgName,
                            Appointment = x.Appointment,
                            UserId = x.UserId,
                            UserFN = x.UserFN,
                            PeriodName = x.PeriodName,
                            planBenefit = x.planBenefit,
                            planExpand = x.planExpand
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanCreditId desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanCreditId).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("План доходов Ф1");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ПЛАН ДОХОДОВ Ф1\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Руководитель";
            ws.Cells["C4"].Value = "Организация";
            ws.Cells["D4"].Value = "Назначение";
            ws.Cells["E4"].Value = "Пользователь";
            ws.Cells["F4"].Value = "Дата";
            ws.Cells["G4"].Value = "Сумма";

            ws.Cells["A4:G4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 7])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("B{0}", row)].Value = item.ChiefName;
                ws.Cells[string.Format("C{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("D{0}", row)].Value = item.Appointment;
                ws.Cells[string.Format("E{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("F{0}", row)].Value = item.Date;
                ws.Cells[string.Format("G{0}", row)].Value = item.Sum;

                row++;
            }

            ws.Cells[string.Format("F{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("F{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("G{0}", row)].Formula = string.Format("SUM(G5:G{0})", row - 1);
            ws.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 6, row, 6])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            //ws.Cells["A:AZ"].AutoFitColumns();

            ws.Column(1).Width = 40;
            ws.Column(1).Style.WrapText = true;

            ws.Column(2).Width = 20;

            ws.Column(3).Width = 20;
            ws.Column(4).Width = 50;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PCF1_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region План расходов Ф1. Экспорт в Excel

        public ActionResult ExportPlanDebitsF1(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from plandebit in db.PlanDebits
                        join prg in db.Projects on plandebit.ProjectId equals prg.id
                        join org in db.Organizations on plandebit.OrganizationId equals org.id
                        join usr in db.IdentityUsers on plandebit.UserId equals usr.Id into usrtmp
                        from usr in usrtmp.DefaultIfEmpty()
                        join pperiod in db.PlanningPeriods on plandebit.PeriodId equals pperiod.PlanningPeriodId
                        where (IDsArray.Contains(plandebit.PlanDebitId))
                        select new
                        {
                            PlanDebitId = plandebit.PlanDebitId,
                            Date = plandebit.Date,
                            Sum = plandebit.Sum,
                            ProjectId = plandebit.ProjectId,
                            OrgId = org.id,
                            Appointment = plandebit.Appointment,
                            UserId = plandebit.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            ProjectName = prg.ShortName,
                            ProjectType = prg.ProjectType,
                            ChiefName = prg.ChiefName,
                            ProjectManagerName = prg.ProjectManagerName,
                            StartDatePlan = prg.StartDatePlan,
                            StartDateFact = prg.StartDateFact,
                            OrgName = org.Title,
                            PeriodName = pperiod.PeriodName,
                            planBenefit = prg.planBenefit,
                            planExpand = prg.planExpand
                        }).AsEnumerable().Select(x => new PlanDebitView
                        {
                            PlanDebitId = x.PlanDebitId,
                            Date = x.Date,
                            Sum = x.Sum,
                            ProjectId = x.ProjectId,
                            ProjectName = x.ProjectName,
                            ProjectType = x.ProjectType,
                            ChiefName = x.ChiefName,
                            ProjectManagerName = x.ProjectManagerName,
                            StartDatePlan = x.StartDatePlan,
                            StartDateFact = x.StartDateFact,
                            OrganizationId = x.OrgId,
                            OrganizationName = x.OrgName,
                            Appointment = x.Appointment,
                            UserId = x.UserId,
                            UserFN = x.UserFN,
                            PeriodName = x.PeriodName,
                            planBenefit = x.planBenefit,
                            planExpand = x.planExpand
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanDebitId desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanDebitId).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("План расходов Ф1");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ПЛАН РАСХОДОВ Ф1\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Руководитель";
            ws.Cells["C4"].Value = "Организация";
            ws.Cells["D4"].Value = "Назначение";
            ws.Cells["E4"].Value = "Пользователь";
            ws.Cells["F4"].Value = "Дата";
            ws.Cells["G4"].Value = "Сумма";

            ws.Cells["A4:G4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 7])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("B{0}", row)].Value = item.ChiefName;
                ws.Cells[string.Format("C{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("D{0}", row)].Value = item.Appointment;
                ws.Cells[string.Format("E{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("F{0}", row)].Value = item.Date;
                ws.Cells[string.Format("G{0}", row)].Value = item.Sum;

                row++;
            }

            ws.Cells[string.Format("F{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("F{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("G{0}", row)].Formula = string.Format("SUM(G5:G{0})", row - 1);
            ws.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 6, row, 6])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            //ws.Cells["A:AZ"].AutoFitColumns();

            ws.Column(1).Width = 40;
            ws.Column(1).Style.WrapText = true;

            ws.Column(2).Width = 20;

            ws.Column(3).Width = 20;
            ws.Column(4).Width = 50;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PDF1_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #endregion

        #region ---------------------- Форма 2 ---------------------------

        #region Входящие платежи Ф2. Экспорт в Excel

        public ActionResult ExportPaymentsF2(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _payments = (from payment in db.PaymentsF2
                             join prg in db.Projects on payment.ProjectId equals prg.id
                             where (IDsArray.Contains(payment.id))
                             select new
                             {
                                 id = payment.id,
                                 WorkerID = payment.WorkerId,
                                 WorkerName = payment.WorkerName,
                                 DocumentNumber = payment.DocumentNumber,
                                 DocumentDate = payment.DocumentDate,
                                 ItemDescr = payment.ItemDescr,
                                 IncomeItems = payment.IncomeItems,
                                 IncomeItemsName = payment.IncomeItemsName,
                                 ProjectId = payment.ProjectId,
                                 ProjectName = payment.ProjectName,
                                 ItemSum = payment.ItemSum,
                                 Currency = payment.Currency,
                                 Receipt = payment.Receipt,
                                 DocumentDescr = payment.DocumentDescr,
                                 Tags = payment.Tags,
                                 ProjectType = prg.ProjectType,
                                 ChiefName = prg.ChiefName,
                                 ProjectManagerName = prg.ProjectManagerName,
                                 StartDatePlan = prg.StartDatePlan,
                                 StartDateFact = prg.StartDateFact
                             }).AsEnumerable().Select(x => new PaymentsF2
                             {
                                 id = x.id,
                                 WorkerId = x.WorkerID,
                                 WorkerName = x.WorkerName,
                                 DocumentNumber = x.DocumentNumber,
                                 DocumentDate = x.DocumentDate,
                                 ItemDescr = x.ItemDescr,
                                 IncomeItems = x.IncomeItems,
                                 IncomeItemsName = x.IncomeItemsName,
                                 ProjectId = x.ProjectId,
                                 ProjectName = x.ProjectName,
                                 ItemSum = x.ItemSum,
                                 Currency = x.Currency,
                                 Receipt = x.Receipt,
                                 DocumentDescr = x.DocumentDescr,
                                 Tags = x.Tags,
                                 ProjectType = x.ProjectType,
                                 ChiefName = x.ChiefName,
                                 ProjectManagerName = x.ProjectManagerName,
                                 StartDatePlan = x.StartDatePlan,
                                 StartDateFact = x.StartDateFact
                             }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
            }
            else
            {
                _payments = _payments.OrderByDescending(x => x.DocumentDate).ThenByDescending(x => x.id).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Входящие платежи Ф2");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ВХОДЯЩИЕ ПЛАТЕЖИ Ф2\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Статья";
            ws.Cells["C4"].Value = "Теги";
            ws.Cells["D4"].Value = "Код";
            ws.Cells["E4"].Value = "Назначение";
            ws.Cells["F4"].Value = "Примечание";
            ws.Cells["G4"].Value = "Квитанция";
            ws.Cells["H4"].Value = "Сотрудник";
            ws.Cells["I4"].Value = "Дата";
            ws.Cells["J4"].Value = "Сумма";
            ws.Cells["K4"].Value = "Валюта";

            ws.Cells["A4:K4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 11])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _payments)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("B{0}", row)].Value = item.IncomeItemsName;
                ws.Cells[string.Format("C{0}", row)].Value = item.Tags;
                ws.Cells[string.Format("D{0}", row)].Value = item.DocumentNumber;
                ws.Cells[string.Format("E{0}", row)].Value = item.ItemDescr;
                ws.Cells[string.Format("F{0}", row)].Value = item.DocumentDescr;
                ws.Cells[string.Format("G{0}", row)].Value = item.Receipt;
                ws.Cells[string.Format("H{0}", row)].Value = item.WorkerName;
                ws.Cells[string.Format("I{0}", row)].Value = item.DocumentDate;
                ws.Cells[string.Format("J{0}", row)].Value = item.ItemSum;
                ws.Cells[string.Format("K{0}", row)].Value = item.Currency;

                row++;
            }

            ws.Cells[string.Format("I{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("I{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("J{0}", row)].Formula = string.Format("SUM(J5:J{0})", row - 1);
            ws.Cells[string.Format("J{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 9, row, 9])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 10, row, 10])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            ws.Cells["A:AZ"].Style.Indent = 1;
            ws.Column(9).Width = 16;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PaymentsF2_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Исходящие платежи Ф2. Экспорт в Excel

        public ActionResult ExportActualDebitsF2(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _payments = (from payment in db.ActualDebitsF2
                             join prg in db.Projects on payment.ProjectId equals prg.id
                             where (IDsArray.Contains(payment.id))
                             select new
                             {
                                 id = payment.id,
                                 WorkerID = payment.WorkerId,
                                 WorkerName = payment.WorkerName,
                                 DocumentNumber = payment.DocumentNumber,
                                 DocumentDate = payment.DocumentDate,
                                 ItemDescr = payment.ItemDescr,
                                 IncomeItems = payment.IncomeItems,
                                 IncomeItemsName = payment.IncomeItemsName,
                                 ProjectId = payment.ProjectId,
                                 ProjectName = payment.ProjectName,
                                 ItemSum = payment.ItemSum,
                                 Currency = payment.Currency,
                                 Receipt = payment.Receipt,
                                 DocumentDescr = payment.DocumentDescr,
                                 Tags = payment.Tags,
                                 ProjectType = prg.ProjectType,
                                 ChiefName = prg.ChiefName,
                                 ProjectManagerName = prg.ProjectManagerName,
                                 StartDatePlan = prg.StartDatePlan,
                                 StartDateFact = prg.StartDateFact
                             }).AsEnumerable().Select(x => new ActualDebitsF2
                             {
                                 id = x.id,
                                 WorkerId = x.WorkerID,
                                 WorkerName = x.WorkerName,
                                 DocumentNumber = x.DocumentNumber,
                                 DocumentDate = x.DocumentDate,
                                 ItemDescr = x.ItemDescr,
                                 IncomeItems = x.IncomeItems,
                                 IncomeItemsName = x.IncomeItemsName,
                                 ProjectId = x.ProjectId,
                                 ProjectName = x.ProjectName,
                                 ItemSum = x.ItemSum,
                                 Currency = x.Currency,
                                 Receipt = x.Receipt,
                                 DocumentDescr = x.DocumentDescr,
                                 Tags = x.Tags,
                                 ProjectType = x.ProjectType,
                                 ChiefName = x.ChiefName,
                                 ProjectManagerName = x.ProjectManagerName,
                                 StartDatePlan = x.StartDatePlan,
                                 StartDateFact = x.StartDateFact
                             }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _payments = _payments.OrderBy(sortColumn + " " + sortColumnDir + ", id desc").ToList();
            }
            else
            {
                _payments = _payments.OrderByDescending(x => x.DocumentDate).ThenByDescending(x => x.id).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Исходящие платежи Ф2");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ИСХОДЯЩИЕ ПЛАТЕЖИ Ф2\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Проект";
            ws.Cells["B4"].Value = "Статья";
            ws.Cells["C4"].Value = "Теги";
            ws.Cells["D4"].Value = "Код";
            ws.Cells["E4"].Value = "Назначение";
            ws.Cells["F4"].Value = "Примечание";
            ws.Cells["G4"].Value = "Квитанция";
            ws.Cells["H4"].Value = "Сотрудник";
            ws.Cells["I4"].Value = "Дата";
            ws.Cells["J4"].Value = "Сумма";
            ws.Cells["K4"].Value = "Валюта";

            ws.Cells["A4:K4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 11])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _payments)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("B{0}", row)].Value = item.IncomeItemsName;
                ws.Cells[string.Format("C{0}", row)].Value = item.Tags;
                ws.Cells[string.Format("D{0}", row)].Value = item.DocumentNumber;
                ws.Cells[string.Format("E{0}", row)].Value = item.ItemDescr;
                ws.Cells[string.Format("F{0}", row)].Value = item.DocumentDescr;
                ws.Cells[string.Format("G{0}", row)].Value = item.Receipt;
                ws.Cells[string.Format("H{0}", row)].Value = item.WorkerName;
                ws.Cells[string.Format("I{0}", row)].Value = item.DocumentDate;
                ws.Cells[string.Format("J{0}", row)].Value = item.ItemSum;
                ws.Cells[string.Format("K{0}", row)].Value = item.Currency;

                row++;
            }

            ws.Cells[string.Format("I{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("I{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("J{0}", row)].Formula = string.Format("SUM(J5:J{0})", row - 1);
            ws.Cells[string.Format("J{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 9, row, 9])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 10, row, 10])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            ws.Cells["A:AZ"].Style.Indent = 1;
            ws.Column(9).Width = 16;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "ADF2_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region План доходов Ф2. Экспорт в Excel

        public ActionResult ExportPlanCreditsF2(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from plancredit in db.PlanCreditsF2
                        join prg in db.Projects on plancredit.ProjectId equals prg.id
                        join org in db.Organizations on plancredit.OrganizationId equals org.id
                        join inc in db.Incomes on plancredit.IncomeId equals inc.Id
                        join usr in db.IdentityUsers on plancredit.UserId equals usr.Id into usrtmp
                        from usr in usrtmp.DefaultIfEmpty()
                        join pperiod in db.PlanningPeriods on plancredit.PeriodId equals pperiod.PlanningPeriodId
                        where (IDsArray.Contains(plancredit.PlanCreditF2Id))
                        select new
                        {
                            PlanCreditF2Id = plancredit.PlanCreditF2Id,
                            Date = plancredit.Date,
                            Sum = plancredit.Sum,
                            ProjectId = plancredit.ProjectId,
                            OrgId = org.id,
                            Appointment = plancredit.Appointment,
                            UserId = plancredit.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            ProjectName = prg.ShortName,
                            ProjectType = prg.ProjectType,
                            ChiefName = prg.ChiefName,
                            ProjectManagerName = prg.ProjectManagerName,
                            StartDatePlan = prg.StartDatePlan,
                            StartDateFact = prg.StartDateFact,
                            OrgName = org.Title,
                            IncId = inc.Id,
                            IncName = inc.Name,
                            PeriodName = pperiod.PeriodName,
                            planBenefit = prg.planBenefit,
                            planExpand = prg.planExpand
                        }).AsEnumerable().Select(x => new PlanCreditViewF2
                        {
                            PlanCreditF2Id = x.PlanCreditF2Id,
                            Date = x.Date,
                            Sum = x.Sum,
                            ProjectId = x.ProjectId,
                            ProjectName = x.ProjectName,
                            ProjectType = x.ProjectType,
                            ChiefName = x.ChiefName,
                            ProjectManagerName = x.ProjectManagerName,
                            StartDatePlan = x.StartDatePlan,
                            StartDateFact = x.StartDateFact,
                            OrganizationId = x.OrgId,
                            OrganizationName = x.OrgName,
                            IncomeId = x.IncId,
                            IncomeName = x.IncName,
                            Appointment = x.Appointment,
                            UserId = x.UserId,
                            UserFN = x.UserFN,
                            PeriodName = x.PeriodName,
                            planBenefit = x.planBenefit,
                            planExpand = x.planExpand
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanCreditF2Id desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanCreditF2Id).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("План доходов Ф2");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ПЛАН ДОХОДОВ Ф2\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Статья";
            ws.Cells["B4"].Value = "Проект";
            ws.Cells["C4"].Value = "Руководитель";
            ws.Cells["D4"].Value = "Организация";
            ws.Cells["E4"].Value = "Назначение";
            ws.Cells["F4"].Value = "Пользователь";
            ws.Cells["G4"].Value = "Дата";
            ws.Cells["H4"].Value = "Сумма";

            ws.Cells["A4:H4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 8])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.IncomeName;
                ws.Cells[string.Format("B{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("C{0}", row)].Value = item.ChiefName;
                ws.Cells[string.Format("D{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("E{0}", row)].Value = item.Appointment;
                ws.Cells[string.Format("F{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("G{0}", row)].Value = item.Date;
                ws.Cells[string.Format("H{0}", row)].Value = item.Sum;

                row++;
            }

            ws.Cells[string.Format("G{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("G{0}", row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[string.Format("H{0}", row)].Formula = string.Format("SUM(H5:H{0})", row - 1);
            ws.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 8, row, 8])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].Style.Indent = 1;
            ws.Cells["A:AZ"].AutoFitColumns();
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PCF2_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region План расходов Ф2. Экспорт в Excel

        public ActionResult ExportPlanDebitsF2(string IDs, string sortColumn, string sortColumnDir)
        {
            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from plandebit in db.PlanDebitsF2
                        join prg in db.Projects on plandebit.ProjectId equals prg.id
                        join org in db.Organizations on plandebit.OrganizationId equals org.id
                        join exp in db.Expenditures on plandebit.ExpenditureId equals exp.Id
                        join usr in db.IdentityUsers on plandebit.UserId equals usr.Id into usrtmp
                        from usr in usrtmp.DefaultIfEmpty()
                        join pperiod in db.PlanningPeriods on plandebit.PeriodId equals pperiod.PlanningPeriodId
                        where (IDsArray.Contains(plandebit.PlanDebitF2Id))
                        select new
                        {
                            PlanDebitF2Id = plandebit.PlanDebitF2Id,
                            Date = plandebit.Date,
                            Sum = plandebit.Sum,
                            ProjectId = plandebit.ProjectId,
                            OrgId = org.id,
                            Appointment = plandebit.Appointment,
                            UserId = plandebit.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                            ProjectName = prg.ShortName,
                            ProjectType = prg.ProjectType,
                            ChiefName = prg.ChiefName,
                            ProjectManagerName = prg.ProjectManagerName,
                            StartDatePlan = prg.StartDatePlan,
                            StartDateFact = prg.StartDateFact,
                            OrgName = org.Title,
                            ExptId = exp.Id,
                            ExpName = exp.Name,
                            PeriodName = pperiod.PeriodName,
                            planBenefit = prg.planBenefit,
                            planExpand = prg.planExpand
                        }).AsEnumerable().Select(x => new PlanDebitViewF2
                        {
                            PlanDebitF2Id = x.PlanDebitF2Id,
                            Date = x.Date,
                            Sum = x.Sum,
                            ProjectId = x.ProjectId,
                            ProjectName = x.ProjectName,
                            ProjectType = x.ProjectType,
                            ChiefName = x.ChiefName,
                            ProjectManagerName = x.ProjectManagerName,
                            StartDatePlan = x.StartDatePlan,
                            StartDateFact = x.StartDateFact,
                            OrganizationId = x.OrgId,
                            OrganizationName = x.OrgName,
                            ExpenditureId = x.ExptId,
                            ExpenditureName = x.ExpName,
                            Appointment = x.Appointment,
                            UserId = x.UserId,
                            UserFN = x.UserFN,
                            PeriodName = x.PeriodName,
                            planBenefit = x.planBenefit,
                            planExpand = x.planExpand
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", PlanDebitF2Id desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.PlanDebitF2Id).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("План расходов Ф2");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ПЛАН РАСХОДОВ Ф2\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Статья";
            ws.Cells["B4"].Value = "Проект";
            ws.Cells["C4"].Value = "Руководитель";
            ws.Cells["D4"].Value = "Организация";
            ws.Cells["E4"].Value = "Назначение";
            ws.Cells["F4"].Value = "Пользователь";
            ws.Cells["G4"].Value = "Дата";
            ws.Cells["H4"].Value = "Сумма";

            ws.Cells["A4:H4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 8])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.ExpenditureName;
                ws.Cells[string.Format("B{0}", row)].Value = item.ProjectName;
                ws.Cells[string.Format("C{0}", row)].Value = item.ChiefName;
                ws.Cells[string.Format("D{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("E{0}", row)].Value = item.Appointment;
                ws.Cells[string.Format("F{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("G{0}", row)].Value = item.Date;
                ws.Cells[string.Format("H{0}", row)].Value = item.Sum;

                row++;
            }

            ws.Cells[string.Format("G{0}", row)].Value = "Итого:";
            ws.Cells[string.Format("G{0}", row)].Style.Font.Bold = true;
            ws.Cells[string.Format("G{0}", row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[string.Format("H{0}", row)].Formula = string.Format("SUM(H5:H{0})", row - 1);
            ws.Cells[string.Format("H{0}", row)].Style.Font.Bold = true;

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 8, row, 8])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].Style.Indent = 1;
            ws.Cells["A:AZ"].AutoFitColumns();
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "PDF2_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #endregion

        #region Остатки на счетах. Экспорт в Excel

        public ActionResult ExportAccountsBalances(string IDs, string sortColumn, string sortColumnDir)
        {

            var IDsArray = new List<int>();
            if (string.IsNullOrEmpty(IDs) || IDs == "[]")
            {
                return Json(new { result = false, message = "Отсутствуют данные для экспорта!" }, JsonRequestBehavior.AllowGet);
            }
            IDsArray = JsonConvert.DeserializeObject<List<int>>(IDs);

            var _ads = (from accbalance in db.AccountsBalances
                        join account in db.Accounts on accbalance.AccountId equals account.AccountId
                        join org in db.Organizations on account.OrganizationId equals org.id
                        join usr in db.IdentityUsers on accbalance.UserId equals usr.Id
                        where (IDsArray.Contains(accbalance.AccountsBalanceId))
                        select new
                        {
                            AccountBalanceId = accbalance.AccountsBalanceId,
                            AccountId = account.AccountId,
                            OrgId = org.id,
                            OrgName = org.Title,
                            BankName = account.BankName,
                            AccountNumber = account.AccountNumber,
                            Date = accbalance.Date,
                            Balance = accbalance.Balance,
                            Note = accbalance.Note,
                            UserId = account.UserId,
                            UserFN = usr.LastName + " " + usr.FirstName.Substring(0, 1) + "." + usr.MiddleName.Substring(0, 1) + ".",
                        }).AsEnumerable().Select(x => new AccountsBalance
                        {
                            AccountsBalanceId = x.AccountBalanceId,
                            AccountId = x.AccountId,
                            OrganizationId = x.OrgId,
                            BankName = x.BankName,
                            AccountNumber = x.AccountNumber,
                            OrganizationName = x.OrgName,
                            Balance = x.Balance,
                            Date = x.Date,
                            Note = x.Note,
                            UserId = x.UserId,
                            UserFN = x.UserFN
                        }).ToList();

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir + ", AccountsBalanceId desc").ToList();
            }
            else
            {
                _ads = _ads.OrderByDescending(x => x.Date).ThenByDescending(x => x.AccountsBalanceId).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Остатки на счетах");

            ws.Cells["A1"].Value = "Название отчета:";
            ws.Cells["B1"].Value = "\"ОСТАТКИ НА СЧЕТАХ\"";
            ws.Cells["A2"].Value = "Дата формирования:";
            ws.Cells["B2"].Value = DateTime.Now;
            ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

            ws.Cells["B1:B2"].Style.Font.Bold = true;
            ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            ws.Cells["A4"].Value = "Организация";
            ws.Cells["B4"].Value = "Банк";
            ws.Cells["C4"].Value = "Счет";
            ws.Cells["D4"].Value = "Примечание";
            ws.Cells["E4"].Value = "Пользователь";
            ws.Cells["F4"].Value = "Дата";
            ws.Cells["G4"].Value = "Сумма";


            ws.Cells["A4:G4"].AutoFilter = true;

            using (ExcelRange col = ws.Cells[4, 1, 4, 7])
            {
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                col.Style.Font.Size = 12;
                col.Style.Font.Bold = true;
                col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
            }

            int row = 5;

            foreach (var item in _ads)
            {
                ws.Cells[string.Format("A{0}", row)].Value = item.OrganizationName;
                ws.Cells[string.Format("B{0}", row)].Value = item.BankName;
                ws.Cells[string.Format("C{0}", row)].Value = item.AccountNumber;
                ws.Cells[string.Format("D{0}", row)].Value = item.Note;
                ws.Cells[string.Format("E{0}", row)].Value = item.UserFN;
                ws.Cells[string.Format("F{0}", row)].Value = item.Date;
                ws.Cells[string.Format("G{0}", row)].Value = item.Balance;

                row++;
            }

            using (ExcelRange col = ws.Cells[5, 6, row, 6])
            {
                col.Style.Numberformat.Format = "dd/MM/yyyy";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            using (ExcelRange col = ws.Cells[5, 7, row, 7])
            {
                col.Style.Numberformat.Format = "#,##0.00";
                col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            ws.Cells["A:AZ"].Style.Indent = 1;
            ws.Cells["A:AZ"].AutoFitColumns();
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 15;

            string path = Server.MapPath("~/DownloadRPT/");
            string repName = "ABalances_" + DateTime.Now.Ticks + ".xlsx";
            pck.SaveAs(new System.IO.FileInfo(path + repName));

            return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region ------------------------- Отчеты -------------------------

        #region Отчет "Бюджетирование". Экспорт в Excel
        public ActionResult ExportAPBReport(int[] IDs
            , string sortColumn
            , string sortColumnDir
            , int Period
            , string ProjectName
            , bool isAllTimes
            , string startPeriod
            , string endPeriod)
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));

                var _isAllTimes = isAllTimes ? 1 : 0;
                string PeriodName = db.PlanningPeriods.FirstOrDefault(x => x.PlanningPeriodId == Period).PeriodName.ToString();

                //Запрос вызывает пользовательскую функцию "ufnAPBReport" хранящуюся на SQL-сервере.
                List<APBTableReport> _ads = db.Database.SqlQuery<APBTableReport>(String.Format("Select * from dbo.ufnAPBReport('{0}', '{1}', {2}, '{3}', {4})", startPeriod, endPeriod, Period, ProjectName, _isAllTimes)).ToList();

                //Проверяем роль пользователя
                bool isAdministrator = System.Web.HttpContext.Current.User.IsInRole("Administrator");
                bool isChief = System.Web.HttpContext.Current.User.IsInRole("Chief");
                bool isAccountant = System.Web.HttpContext.Current.User.IsInRole("Accountant");
                bool isFinancier = System.Web.HttpContext.Current.User.IsInRole("Financier");
                decimal? _PlanningBalance = null;
                if (isAdministrator || isChief || isAccountant || isFinancier)
                {
                    //Запрос вызывает пользовательскую функцию "ufnPlanningBalance" хранящуюся на SQL-сервере.
                    _PlanningBalance = db.Database.SqlQuery<decimal>("Select dbo.ufnPlanningBalance() as PlanningBalance").FirstOrDefault();
                }

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"

                _ads = _ads.Where(x =>
                            (IDs == null
                            || IDs.Length == 0
                            || IDs.Contains(x.prj))
                            && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(x.Chief)) //Фильтрация записей по проектам для руководителей проектов
                            ).ToList();

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderByDescending(x => x.ProjectName).ToList();
                }

                //---------------------EXPORT----------------------//

                ExcelPackage pck = new ExcelPackage();
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Бюджетирование");

                ws.Cells["A1"].Value = "Название отчета:";
                ws.Cells["B1"].Value = "\"КОНТРОЛЬ ВЫПОЛНЕНИЯ БЮДЖЕТОВ\"";
                ws.Cells["A2"].Value = "Дата формирования:";
                ws.Cells["B2"].Value = DateTime.Now;
                ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                
                string _sd = DateTime.ParseExact(startPeriod, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                string _ed = DateTime.ParseExact(endPeriod, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");

                ws.Cells["A3"].Value = "Отчетный период:";
                ws.Cells["B3"].Value = _sd + " - " + _ed;

                if (_PlanningBalance != null)
                {
                    ws.Cells["A4"].Value = "Плановый остаток:";
                    ws.Cells["B4"].Value = _PlanningBalance;
                    ws.Cells["B4"].Style.Numberformat.Format = "#,##0.00";
                }

                ws.Cells["B1:B4"].Style.Font.Bold = true;
                ws.Cells["B2:B4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["A6"].Value = "Проект";
                ws.Cells["B6"].Value = "Финансовый результат";
                ws.Cells["C6"].Value = "Доходы (план)";
                ws.Cells["D6"].Value = "Доходы (факт)";
                ws.Cells["E6"].Value = "\u0394 дох.";
                ws.Cells["F6"].Value = "Расходы (план)";
                ws.Cells["G6"].Value = "Расходы (факт)";
                ws.Cells["H6"].Value = "\u0394 расх.";

                ws.Cells["A6:H6"].AutoFilter = true;

                using (ExcelRange col = ws.Cells[6, 1, 6, 8])
                {
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    col.Style.Font.Size = 12;
                    col.Style.Font.Bold = true;
                    col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                }

                int row = 7;

                foreach (var item in _ads)
                {
                    ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                    ws.Cells[string.Format("B{0}", row)].Value = item.prjres;
                    ws.Cells[string.Format("C{0}", row)].Value = item.debitplan;
                    ws.Cells[string.Format("D{0}", row)].Value = item.debitfact;
                    ws.Cells[string.Format("E{0}", row)].Formula = string.Format("D{0}-C{0}", row);
                    ws.Cells[string.Format("F{0}", row)].Value = item.creditplan;
                    ws.Cells[string.Format("G{0}", row)].Value = item.creditfact;
                    ws.Cells[string.Format("H{0}", row)].Formula = string.Format("G{0}-F{0}", row);

                    row++;
                }

                // Условное форматирование итоговых и дельта-ячеек
                var cfRule1_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("B7:B{0}", row)]);
                cfRule1_1.Formula = "0";
                cfRule1_1.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#D2E7BE");

                var cfRule1_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("B7:B{0}", row)]);
                cfRule1_2.Formula = "0";
                cfRule1_2.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#F8C9D3");

                var cfRule2_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("E7:E{0}", row)]);
                cfRule2_1.Formula = "0";
                cfRule2_1.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#D2E7BE");

                var cfRule2_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("E7:E{0}", row)]);
                cfRule2_2.Formula = "0";
                cfRule2_2.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#F8C9D3");

                var cfRule3_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("H7:H{0}", row)]);
                cfRule3_1.Formula = "0";
                cfRule3_1.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#F8C9D3");

                var cfRule3_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("H7:H{0}", row)]);
                cfRule3_2.Formula = "0";
                cfRule3_2.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#D2E7BE");



                //Итоги
                ws.Cells[string.Format("A{0}:B{0}", row)].Merge = true;
                ws.Cells[string.Format("A{0}", row)].Value = "Итого";
                ws.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
                ws.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[string.Format("C{0}", row)].Formula = string.Format("SUM(C5:C{0})", row - 1);
                ws.Cells[string.Format("D{0}", row)].Formula = string.Format("SUM(D5:D{0})", row - 1);
                ws.Cells[string.Format("E{0}", row)].Formula = string.Format("SUM(E5:E{0})", row - 1);
                ws.Cells[string.Format("F{0}", row)].Formula = string.Format("SUM(F5:F{0})", row - 1);
                ws.Cells[string.Format("G{0}", row)].Formula = string.Format("SUM(G5:G{0})", row - 1);
                ws.Cells[string.Format("H{0}", row)].Formula = string.Format("SUM(H5:H{0})", row - 1);

                using (ExcelRange col = ws.Cells[row, 3, row, 8])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    col.Style.Font.Bold = true;
                }

                using (ExcelRange col = ws.Cells[7, 2, row, 8])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                using (ExcelRange col = ws.Cells[6, 1, row, 8])
                {
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                ws.Cells["A:AZ"].Style.Indent = 1;
                ws.Cells["B6"].Style.WrapText = true;
                ws.Row(6).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A:AZ"].AutoFitColumns();
                ws.Column(2).Width = 23;
                ws.Column(5).Width = 20;
                ws.Column(8).Width = 20;

                string path = Server.MapPath("~/DownloadRPT/");
                string repName = "APBReport_" + DateTime.Now.Ticks + ".xlsx";
                pck.SaveAs(new System.IO.FileInfo(path + repName));

                return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Ошибка выполнения запроса: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Отчет "Прибыльность проектов". Экспорт в Excel
        public ActionResult ExportAPPReport(int[] IDs
            , string sortColumn
            , string sortColumnDir
            , string repDate)
        {
            try
            {
                db.Database.Log = (s => System.Diagnostics.Debug.WriteLine(s));


                //Запрос вызывает пользовательскую функцию "ufnAPBReport" хранящуюся на SQL-сервере.
                List<APPTableReport> _ads = db.Database.SqlQuery<APPTableReport>(String.Format("Select * from dbo.ufnAPPReport('{0}', '{1}') ORDER BY prj DESC", repDate, "")).ToList();

                List<int> WorkerIdsList = UsrWksMethods.GetWorkerId(db); // Получаем ID связанных сотрудников для пользователя в роли "Руководитель проекта"

                _ads = _ads.Where(x =>
                            (IDs == null
                            || IDs.Length == 0
                            || IDs.Contains(x.prj))
                            && (WorkerIdsList.FirstOrDefault() == -1 || WorkerIdsList.Contains(x.Chief)) //Фильтрация записей по проектам для руководителей проектов
                            ).ToList();

                if (sortColumn == "ProjectName")
                {
                    _ads = _ads.OrderBy("IPA " + sortColumnDir).ToList();
                }
                else
                {
                    _ads = _ads.OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                //---------------------EXPORT----------------------//

                ExcelPackage pck = new ExcelPackage();
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Прибыльность");

                ws.Cells[string.Format("B{0}:C{0}", 1)].Merge = true;
                ws.Cells[string.Format("B{0}:C{0}", 2)].Merge = true;
                ws.Cells[string.Format("B{0}:C{0}", 3)].Merge = true;

                ws.Cells["A1"].Value = "Название отчета:";
                ws.Cells["B1"].Value = "\"АНАЛИЗ ПРИБЫЛЬНОСТИ ПРОЕКТОВ\"";
                ws.Cells["A2"].Value = "Дата формирования:";
                ws.Cells["B2"].Value = DateTime.Now;
                ws.Cells["B2"].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                ws.Cells["A3"].Value = "Отчетная дата:";
                ws.Cells["B3"].Value = DateTime.ParseExact(repDate, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");

                ws.Cells["B1:B4"].Style.Font.Bold = true;
                ws.Cells["B2:B4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["A5"].Value = "Проект";
                ws.Cells["B5"].Value = "Доходы Ф1";
                ws.Cells["C5"].Value = "Доходы Ф2";
                ws.Cells["D5"].Value = "\u03A3 дох.";
                ws.Cells["E5"].Value = "Расходы Ф1";
                ws.Cells["F5"].Value = "Расходы Ф2";
                ws.Cells["G5"].Value = "\u03A3 расх.";
                ws.Cells["H5"].Value = "Прибыль Ф1";
                ws.Cells["I5"].Value = "Прибыль Ф2";
                ws.Cells["J5"].Value = "Прибыльность";

                ws.Cells["A5:J5"].AutoFilter = true;

                using (ExcelRange col = ws.Cells[5, 1, 5, 11])
                {
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    col.Style.Font.Size = 12;
                    col.Style.Font.Bold = true;
                    col.Style.Font.Color.SetColor(System.Drawing.Color.DarkGreen);
                }

                int row = 6;

                foreach (var item in _ads)
                {
                    ws.Cells[string.Format("A{0}", row)].Value = item.ProjectName;
                    ws.Cells[string.Format("B{0}", row)].Value = item.FactCreditF1;
                    ws.Cells[string.Format("C{0}", row)].Value = item.FactCreditF2;
                    ws.Cells[string.Format("D{0}", row)].Formula = string.Format("B{0}+C{0}", row);
                    ws.Cells[string.Format("E{0}", row)].Value = item.FactDebitF1;
                    ws.Cells[string.Format("F{0}", row)].Value = item.FactDebitF2;
                    ws.Cells[string.Format("G{0}", row)].Formula = string.Format("E{0}+F{0}", row);
                    ws.Cells[string.Format("H{0}", row)].Formula = string.Format("B{0}-E{0}", row);
                    ws.Cells[string.Format("I{0}", row)].Formula = string.Format("C{0}-F{0}", row);
                    ws.Cells[string.Format("J{0}", row)].Formula = string.Format("H{0}+I{0}", row);
                    row++;
                }

                // Условное форматирование итоговых и дельта-ячеек
                var cfRule1_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("H6:H{0}", row)]);
                cfRule1_1.Formula = "0";
                cfRule1_1.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#D2E7BE");

                var cfRule1_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("H6:H{0}", row)]);
                cfRule1_2.Formula = "0";
                cfRule1_2.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#F8C9D3");

                var cfRule2_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("I6:I{0}", row)]);
                cfRule2_1.Formula = "0";
                cfRule2_1.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#D2E7BE");

                var cfRule2_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("I6:I{0}", row)]);
                cfRule2_2.Formula = "0";
                cfRule2_2.Style.Fill.BackgroundColor.Color = ColorTranslator.FromHtml("#F8C9D3");

                var cfRule3_1 = ws.ConditionalFormatting.AddGreaterThan(ws.Cells[string.Format("J6:J{0}", row)]);
                cfRule3_1.Formula = "0";
                cfRule3_1.Style.Font.Color.Color = ColorTranslator.FromHtml("#006400");

                var cfRule3_2 = ws.ConditionalFormatting.AddLessThan(ws.Cells[string.Format("J6:J{0}", row)]);
                cfRule3_2.Formula = "0";
                cfRule3_2.Style.Font.Color.Color = ColorTranslator.FromHtml("#b22222");

                //Итоги
                ws.Cells[string.Format("A{0}", row)].Value = "Итого";
                ws.Cells[string.Format("A{0}", row)].Style.Font.Bold = true;
                ws.Cells[string.Format("A{0}", row)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[string.Format("B{0}", row)].Formula = string.Format("SUM(B5:B{0})", row - 1);
                ws.Cells[string.Format("C{0}", row)].Formula = string.Format("SUM(C5:C{0})", row - 1);
                ws.Cells[string.Format("D{0}", row)].Formula = string.Format("SUM(D5:D{0})", row - 1);
                ws.Cells[string.Format("E{0}", row)].Formula = string.Format("SUM(E5:E{0})", row - 1);
                ws.Cells[string.Format("F{0}", row)].Formula = string.Format("SUM(F5:F{0})", row - 1);
                ws.Cells[string.Format("G{0}", row)].Formula = string.Format("SUM(G5:G{0})", row - 1);
                ws.Cells[string.Format("H{0}", row)].Formula = string.Format("SUM(H5:H{0})", row - 1);
                ws.Cells[string.Format("I{0}", row)].Formula = string.Format("SUM(I5:I{0})", row - 1);
                ws.Cells[string.Format("J{0}", row)].Formula = string.Format("SUM(J5:J{0})", row - 1);

                using (ExcelRange col = ws.Cells[6, 4, row, 4])
                {
                    col.Style.Font.Bold = true;
                }

                using (ExcelRange col = ws.Cells[6, 7, row, 7])
                {
                    col.Style.Font.Bold = true;
                }

                using (ExcelRange col = ws.Cells[6, 10, row, 10])
                {
                    col.Style.Font.Bold = true;
                }

                using (ExcelRange col = ws.Cells[row, 2, row, 10])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    col.Style.Font.Bold = true;
                }

                using (ExcelRange col = ws.Cells[6, 2, row, 10])
                {
                    col.Style.Numberformat.Format = "#,##0.00";
                    col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                using (ExcelRange col = ws.Cells[5, 1, row, 10])
                {
                    col.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    col.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                ws.Cells["A:AZ"].Style.Indent = 1;
                ws.Cells["B6"].Style.WrapText = true;
                ws.Row(6).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A:AZ"].AutoFitColumns();
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 20;
                ws.Column(5).Width = 20;
                ws.Column(6).Width = 20;
                ws.Column(7).Width = 20;
                ws.Column(8).Width = 20;
                ws.Column(9).Width = 20;
                ws.Column(10).Width = 20;

                string path = Server.MapPath("~/DownloadRPT/");
                string repName = "APBReport_" + DateTime.Now.Ticks + ".xlsx";
                pck.SaveAs(new System.IO.FileInfo(path + repName));

                return Json(new { filename = repName, result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Ошибка выполнения запроса: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #endregion

        #region Оригинал. Экспорт в excel

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
        #endregion

        #region Метод загрузки файла отчета
        [DeleteFile] // Фильтр удаления файла после загрузки
        public ActionResult GetFile(string FName)
        {
            try
            {
                // Путь к файлу
                string file_path = Server.MapPath("~/DownloadRPT/");
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
        #endregion

        #region Фильтр удаления файла после загрузки
        public class DeleteFile : ActionFilterAttribute
        {
            public override void OnResultExecuted(ResultExecutedContext filterContext)
            {
                filterContext.HttpContext.Response.Flush();
                var filePathResult = filterContext.Result as FilePathResult;
                if (filePathResult != null)
                {
                    System.IO.File.Delete(filePathResult.FileName);
                }
            }
        }
        #endregion


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