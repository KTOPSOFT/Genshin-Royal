using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Shop.Paypal
{
    /// <summary>
    /// Body request of the paypal order for authorization
    /// </summary>
    [Serializable]
    public class bl_PaypalOrder
    {
        public string intent;
        public List<PurchaseUnit> purchase_units;
        public ApplicationContext application_context;


        public bl_PaypalOrder()
        {
            intent = "CAPTURE";
            purchase_units = new List<PurchaseUnit>();
            purchase_units.Add(new PurchaseUnit()
            {
                description = $"{Application.productName} digital purchase.",
                amount = new Amount()
                {
                    currency_code = bl_PaypalSettings.Instance.currencyCode.ToString(),
                    breakdown = new AmountBreakdown()
                    {
                        item_total = new Money(),
                        shipping = new Money()
                    }
                },
                items = new List<Item>()
            });

            application_context = new ApplicationContext()
            {
                user_action = "PAY_NOW",
                shipping_preference = "NO_SHIPPING",
                brand_name = bl_PaypalSettings.Instance.bussinesName,
                return_url = bl_PaypalSettings.Instance.redirectURL,
                cancel_url = bl_PaypalSettings.Instance.cancelURL
            };
        }

        /// <summary>
        /// Add item to the order request
        /// </summary>
        public void AddUnit(string itemName, string itemDescription, float value, int itemQuantity = 1)
        {
            var purchaseData = purchase_units[0];

            purchaseData.items.Add(new Item()
            {
                name = itemName,
                quantity = itemQuantity,
                description = itemDescription,
                rawAmount = value,
                category = "DIGITAL_GOODS",
                unit_amount = new Money()
                {
                    currency_code = purchaseData.amount.currency_code,
                    value = value.ToString().Replace(",", "."),
                }
            });

            float v = 0;
            for (int i = 0; i < purchaseData.items.Count; i++)
            {
                v += (purchaseData.items[i].rawAmount * purchaseData.items[i].quantity);
            }
            purchaseData.amount.value = v.ToString("0.00").Replace(",",".");
            purchaseData.amount.breakdown.item_total.value = purchaseData.amount.value;
        }

        [Serializable]
        public class PurchaseUnit
        {
            public Amount amount;
            public List<Item> items;
            public string description;
        }

        [Serializable]
        public class Amount
        {
            public string currency_code;
            public string value;
            public AmountBreakdown breakdown;
        }

        [Serializable]
        public class Money
        {
            public string currency_code;
            public string value;

            public Money()
            {
                currency_code = bl_PaypalSettings.Instance.currencyCode.ToString();
                value = "0";
            }
        }

        [Serializable]
        public class Item
        {
            public string name;
            public Money unit_amount;
            public int quantity;
            public string description;
            public string category = "DIGITAL_GOODS";
            [NonSerialized] public float rawAmount;
        }

        [Serializable]
        public class ApplicationContext
        {
            public string brand_name;
            public string shipping_preference = "NO_SHIPPING";
            public string user_action = "PAY_NOW";
            public string return_url;
            public string cancel_url;
        }

        [Serializable]
        public class AmountBreakdown
        {
            public Money item_total;
            public Money shipping;
        }
    }
}