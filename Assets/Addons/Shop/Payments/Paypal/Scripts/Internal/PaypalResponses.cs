using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Shop.Paypal
{
    /// <summary>
    /// Token received after requesting with the clientID
    /// </summary>
    [Serializable]
    public class AccessTokenResponse
    {
        public string scope;
        public string access_token;
        public string token_type;
        public string app_id;
        public int expires_in;
        public string nonce;
    }

    /// <summary>
    /// Order capture information
    /// This contain the approve order info retrieve by the paypal server
    /// </summary>
    [Serializable]
    public class CapturePayment
    {
        public string id;
        public List<Link> links;
        public string status;

        public bool isCreated => status == "CREATED";
        public string GetApproveURL => links.Find(x => x.rel == "approve").href;

        [Serializable]
        public class Link
        {
            public string href;
            public string rel;
            public string method;
        }
    }

    [Serializable]
    public class OrderCaptureResponse
    {
        public string id;
        public string status;
        public Payer payer;
        public List<PurchaseUnit> purchase_units;
        public List<Link2> links;

        public bool isCompleted => status == "COMPLETED";

        public string GetMinimalReceipt()
        {
            var r = new Receipt()
            {
                orderID = id,
                payerName = payer.name.given_name,
                paymentType = "Paypal",
            };
            return JsonUtility.ToJson(r);
        }

        [Serializable]
        public class Receipt
        {
            public string orderID;
            public string payerName;
            public string paymentType;
        }

        [Serializable]
        public class Name
        {
            public string given_name;
            public string surname;
        }

        [Serializable]
        public class Payer
        {
            public Name name;
            public string email_address;
            public string payer_id;
        }

        [Serializable]
        public class Address
        {
            public string address_line_1;
            public string address_line_2;
            public string admin_area_2;
            public string admin_area_1;
            public string postal_code;
            public string country_code;
        }

        [Serializable]
        public class Shipping
        {
            public Address address;
        }

        [Serializable]
        public class Amount
        {
            public string currency_code;
            public string value;
        }

        [Serializable]
        public class SellerProtection
        {
            public string status;
            public List<string> dispute_categories;
        }

        [Serializable]
        public class GrossAmount
        {
            public string currency_code;
            public string value;
        }

        [Serializable]
        public class PaypalFee
        {
            public string currency_code;
            public string value;
        }

        [Serializable]
        public class NetAmount
        {
            public string currency_code;
            public string value;
        }

        [Serializable]
        public class SellerReceivableBreakdown
        {
            public GrossAmount gross_amount;
            public PaypalFee paypal_fee;
            public NetAmount net_amount;
        }

        [Serializable]
        public class Link
        {
            public string href;
            public string rel;
            public string method;
        }

        [Serializable]
        public class Capture
        {
            public string id;
            public string status;
            public Amount amount;
            public SellerProtection seller_protection;
            public bool final_capture;
            public string disbursement_mode;
            public SellerReceivableBreakdown seller_receivable_breakdown;
            public DateTime create_time;
            public DateTime update_time;
            public List<Link> links;
        }

        [Serializable]
        public class Payments
        {
            public List<Capture> captures;
        }

        [Serializable]
        public class PurchaseUnit
        {
            public string reference_id;
            public Shipping shipping;
            public Payments payments;
        }

        [Serializable]
        public class Link2
        {
            public string href;
            public string rel;
            public string method;
        }
    }
}