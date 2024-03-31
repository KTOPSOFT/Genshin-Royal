using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MFPS.ULogin
{
    public class JsonULoginStructures
    {

    }

    [Serializable]
    public class CoinPurchaseData
    {
        public string productID;
        public int coins;
        public int coinID;
        public string receipt;
    }

    [Serializable]
    public class PurchaseReceipt
    {
        public string method;
        public string orderid;
        public string transid;

        public PurchaseReceipt(string Method, string OrderId, string TransactionId)
        {
            method = Method;
            orderid = OrderId;
            transid = TransactionId;
        }
    }
}