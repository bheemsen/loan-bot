using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeLoanCalculator.Models
{
    [Serializable]
    public class EMIModel
    {
        public double ProjectCosts { get; set; }
        public double OwnResources { get; set; }
        public double HousingSubsidies { get; set; }
        public double financingAmount { get; set; }
        public double MonthlyInstallment { get; set; }
        public int NumberOfYears { get; set; }
    }
}