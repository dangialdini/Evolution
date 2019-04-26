using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.MediaService;
using Evolution.AuditService;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService {
        public ProductIPListModel FindProductIPListModel(int productId, int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue) {
            var model = new ProductIPListModel();

            // Get all the markets
            var allItems = db.FindLOVItems(null, LOVName.MarketingLocation);
            var ipList = db.FindProductIPs(productId);

            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                var newItem = new ProductIPModel();

                var temp = ipList.Where(il => il.MarketId == item.Id).FirstOrDefault();
                if (temp != null) {
                    // Product has the market applied
                    newItem.Id = temp.Id;
                    newItem.Selected = true;
                }
                newItem.ProductId = productId;
                newItem.MarketId = item.Id;
                newItem.MarketNameText = item.ItemText;

                model.Items.Add(newItem);
            }
            return model;
        }

        public Error InsertOrUpdateProductIP(ProductModel product, ProductIPListModel productIPs, string lgs) {
            var error = new Error();

            var currentIPs = db.FindProductIPs(product.Id).ToList();

            // First, handles items which are new or existing/selected
            foreach(var item in productIPs.Items.Where(pi => pi.Selected == true)) {
                var temp = currentIPs.Where(ip => ip.MarketId == item.MarketId).FirstOrDefault();
                if(temp == null) {
                    // Not found in current IPs so has been added
                    var productIp = new ProductIP {
                        ProductId = product.Id,
                        MarketId = item.MarketId
                    };
                    db.InsertOrUpdateProductIP(productIp);
                } else {
                    // Was found, so remove from list
                    currentIPs.Remove(temp);
                }
            }

            // Now handle items remaining in the db as these are surplus
            db.DeleteProductIPs(currentIPs);

            return error;
        }

        public Error DeleteProductIP(int id) {
            var error = new Error();

            var p = db.FindProductIP(id);
            if (p != null) {
                db.DeleteProductIP(id);
            }
            return error;
        }

        public ProductIPModel MapToModel(ProductIP item) {
            return Mapper.Map<ProductIP, ProductIPModel>(item);
        }
    }
}
