using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.ViewModels;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        public decimal CalculateEstimatedFreight(SalesOrderHeaderModel soh, CustomerModel customer) {
            decimal freightValue = 0,
                    totalOrderValue = 0,
                    minFreightPerOrder = (soh.MinFreightPerOrder == null ? 0 : soh.MinFreightPerOrder.Value),
                    freightRate = (soh.FreightRate == null ? 0 : soh.FreightRate.Value),
                    minFreightThreshold = (customer.MinFreightThreshold == null ? 0 : customer.MinFreightThreshold.Value),
                    freightWhenBelowThreshold = (customer.FreightWhenBelowThreshold == null ? 0 : customer.FreightWhenBelowThreshold.Value);
            //string  rateCalcMethod = "";

            foreach(var item in db.FindSalesOrderDetails(soh.CompanyId, soh.Id)) {
                if(item.LineStatusId == (int)SalesOrderLineStatus.Unpicked ||
                   item.LineStatusId == (int)SalesOrderLineStatus.PickingInProgress ||
                   item.LineStatusId == (int)SalesOrderLineStatus.SentForPicking) {

                    totalOrderValue += (item.UnitPriceExTax == null ? 0 : item.UnitPriceExTax.Value) *
                                       (item.OrderQty == null ? 0 : item.OrderQty.Value) *
                                       1 - (item.DiscountPercent == null ? 0 : item.DiscountPercent.Value);
                }
            }

            if (minFreightPerOrder > 0) {
                if (freightRate > 0) {
                    // If the order specifies a min value per order AND a freight rate,
                    // calculate the freight value based on the larger of the two
                    if (minFreightPerOrder > totalOrderValue * freightRate) {
                        // Min value > freight rate. Charge the entire freight rate.
                        freightValue = minFreightPerOrder;
                        //rateCalcMethod = "Min freight";
                    } else {
                        // Min value < freight rate. Charge the freight rate
                        freightValue = totalOrderValue * freightRate;
                        //rateCalcMethod = "Freight rate";
                    }
                } else {
                    // A min value is supplied, but a freight rate is not. Charge the min value
                    freightValue = minFreightPerOrder;
                    //rateCalcMethod = "Min freight";
                }
            } else if(freightRate > 0) {
                // A min value is not supplied, but a freight rate is. Charge the freight rate
                freightValue = totalOrderValue * freightRate;
                //rateCalcMethod = "Freight rate";
            } else {
                // Nothing is supplied. Charge $0.00 freight
                freightValue = 0;
                //rateCalcMethod = "No Freight";
            }

            // Calculate the pick freight value
            if (totalOrderValue < minFreightThreshold) {
                // If the total order value is less than the minimum threshold, then 
                // use FreightWhenBelowThreshold for this order.
                freightValue = freightWhenBelowThreshold;
                //rateCalcMethod = "Below min threshold";
            } else if (totalOrderValue < minFreightThreshold) {
                freightValue = 0;
                //rateCalcMethod = "Below min threshold";
            }

            return freightValue;
        }
    }
}
