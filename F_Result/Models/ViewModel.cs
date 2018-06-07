using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace F_Result.Models
{

    #region "Analysis of the project budget" table report ----------------------------------

    [NotMapped]
    public class APBTableReport
    {
        public int prj { get; set; }
        public string ProjectName { get; set; }
        public decimal prjres { get; set; }
        public string ProjectType { get; set; }
        public string ProjectManagerName { get; set; }
        public string ChiefName { get; set; }
        public int Chief { get; set; }
        public DateTime? StartDateFact { get; set; }
        public DateTime? StartDatePlan { get; set; }
        public decimal debitplan { get; set; }
        public decimal debitfact { get; set; }
        public decimal ddelta { get; set; }
        public decimal creditplan { get; set; }
        public decimal creditfact { get; set; }
        public decimal cdelta { get; set; }
        public decimal planBenefit { get; set; }
        public decimal planExpand { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime? IPA { get; set; }
    }

    [NotMapped]
    public class APBTableReportTotal
    {
        public decimal DebitPlanTotal { get; set; }
        public decimal DebitFactTotal { get; set; }
        public decimal dDeltaTotal { get; set; }
        public decimal CreditPlanTotal { get; set; }
        public decimal CreditFactTotal { get; set; }
        public decimal cDeltaTotal { get; set; }
    }

    [NotMapped]
    public class APBFilterIDs
    {
        public int PrjId { get; set; }
        public string ProjectName { get; set; }
        public DateTime? IPA { get; set; }
    }

    #endregion ----------------------------------------------------------

    #region "Analysis of the project Profitability" table report -------------------
    [NotMapped]
    public class APPTableReport
    {
        public int prj { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ProjectManagerName { get; set; }
        public string ChiefName { get; set; }
        public int Chief { get; set; }
        public decimal FactCreditF1 { get; set; }
        public decimal FactCreditF2 { get; set; }
        public decimal FCTotalF1F2 { get; set; }
        public decimal FactDebitF1 { get; set; }
        public decimal FactDebitF2 { get; set; }
        public decimal FDTotalF1F2 { get; set; }
        public decimal IncomeF1 { get; set; }
        public decimal IncomeF2 { get; set; }
        public decimal IncomeTotal { get; set; }
        public DateTime? IPA { get; set; }
        public DateTime? StartDateFact { get; set; }
        public DateTime? StartDatePlan { get; set; }
        public decimal planBenefit { get; set; }
        public decimal planExpand { get; set; }

    }

    [NotMapped]
    public class APPTableReportTotal
    {
        public decimal FactCreditF1Total { get; set; }
        public decimal FactCreditF2Total { get; set; }
        public decimal FCF1F2Total { get; set; }
        public decimal FactDebitF1Total { get; set; }
        public decimal FactDebitF2Total { get; set; }
        public decimal FDF1F2Total { get; set; }
        public decimal IncomeF1Total { get; set; }
        public decimal IncomeF2Total { get; set; }
        public decimal IncomeTotal { get; set; }
    }

    #endregion

}