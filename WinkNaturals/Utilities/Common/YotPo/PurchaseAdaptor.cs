using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Utilities.Common.YotPo.Domain;

namespace WinkNaturals.Utilities.Common.YotPo
{

    public class PurchaseAdaptor
    {
        private readonly IOptions<ConfigSettings> _config;
        public PurchaseAdaptor()
        {

        }
        public PurchaseAdaptor(IOptions<ConfigSettings> config)
        {
            _config = config;
        }
        public PurchaseRequest CreatePurchaseRequest(string orderId, string customerName, string email, string productBaseurl, List<Item> items)
        {
            var purchaseRequest = new PurchaseRequest
            {
                ValidateData = true,
                CurrencyIso = "USD",
                Platform = _config.Value.YotPo.Platform,//GlobalSettings.YotPo.Platform,
                CustomerName = customerName,
                Email = email,
                OrderId = orderId,
                OrderDate = DateTime.Now.ToString("yyyy-MM-dd")
            };
            var products = new Dictionary<string, Product>();

            foreach (var item in items)
            {
                var product = new Product
                {
                    Name = item.ItemDescription,
                    Description = item.LongDetail1,
                    ImageUrl = item.SmallImageUrl,
                    Url = productBaseurl + item.ItemCode,
                    Price = item.Price.ToString()
                };
                products[item.ItemCode] = product;
            }
            purchaseRequest.Products = products;
            return purchaseRequest;
        }
    }
}