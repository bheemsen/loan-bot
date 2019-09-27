using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeLoanCalculator.Models
{
    public class ClientResponse
    {
        public string Legend { get; set; }
        public double InstallmentAmount { get; set; }
        public double InstallmentFixed { get; set; }
        public  double InstallmentInternal { get; set; }
        public double LoanAmount { get; set; }
        public string Notiz { get; set; }

    }
}