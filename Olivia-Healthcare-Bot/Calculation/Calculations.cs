using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class Calculations
    {
        public static double CalculateEMI(double amount, int year)
        {
            return amount * year;
        }

        public static double CalculatePurchaseCost(double purchasePriceOfLand)
        {
            return purchasePriceOfLand - 100;
        }

        public static double CalculateRealEstateTax(double purchasePriceOfLand)
        {
            return (purchasePriceOfLand * 5)/100;
        }

        public static double CalculateEntryPropertyRights(double purchasePriceOfLand)
        {
            return (purchasePriceOfLand * 70) / 100;
        }
        public static double CalculateBrockerageCommission(double purchasePriceOfLand)
        {
            return (purchasePriceOfLand * 3) / 100;
        }
    }
}
