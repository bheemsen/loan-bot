using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeLoanCalculator.Models
{
    public class ProjectCosts
    {
        public double PurchasePriceOfLand { get; set; }
        public double BuildingCosts { get; set; }
        public double FinancingCost { get; set; }
        public double OthrCosts { get; set; }

        public double PurchaseCosts
        {
            get {
                return PurchaseCosts;
            }
            set {
                PurchaseCosts = PurchasePriceOfLand * 0.02;
            }
        }
        public double RealEstateCost
        {
            get
            {
                return RealEstateCost;
            }

            set
            {
                RealEstateCost = PurchasePriceOfLand * 0.035;
            }
        }
        public double EntryPropertyRights
        { get
            {
                return EntryPropertyRights;
            }
            set
            {
                EntryPropertyRights = PurchasePriceOfLand * 0.011;
            }
        }
        public double BroerageCommission
        { get
            {
                return BroerageCommission;
            }
            set
            {
                double result = 0.0;
                if (PurchasePriceOfLand <= 36336) { result = PurchasePriceOfLand * 0.04; }

                if (PurchasePriceOfLand > 36336 && PurchasePriceOfLand <= 48448) { result = PurchasePriceOfLand * 0.03; }

                if (PurchasePriceOfLand <= 1453) { result = PurchasePriceOfLand * 0.20; }
                BroerageCommission = result;
            }
        }
    }
}